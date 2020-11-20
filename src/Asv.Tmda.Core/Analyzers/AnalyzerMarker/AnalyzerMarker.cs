using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tmda.Core.ILS;
using NLog;

namespace Asv.Tmda.Core.Marker
{
    public class AnalyzerMarkerKalmanFilter
    {
        public double MeasurementNoise { get; set; } = 0.1;
        public double ResetFilterCondition { get; set; } = 2;
    }

    public class AnalyzerMarkerConfig
    {
        public double Bandwidth { get; set; } = 7_000;
        public double SampleRate { get; set; } = 9765;
        public double FrequencyHz { get; set; } = 75_000_000;
        public WindowFilterEnum FftWindowFilter { get; set; } = WindowFilterEnum.Hamming;
        public int FftSize { get; set; } = 9765 / 20;
        public int AmHistorySize { get; set; } = 50;
        public AnalyzerMarkerKalmanFilter AmKalman { get; } = new AnalyzerMarkerKalmanFilter{ MeasurementNoise = 0.5, ResetFilterCondition = 1};
        public AnalyzerMarkerKalmanFilter LevelKalman { get; } = new AnalyzerMarkerKalmanFilter { MeasurementNoise = 0.5, ResetFilterCondition = 2 };
    }

    public class MarkerValue
    {
        public double Am400 { get; set; }
        public double Am1300 { get; set; }
        public double Am3000 { get; set; }
        public TimeSpan ReadDataTime { get; set; }
        public TimeSpan AllTime { get; set; }
        public double FreqHz { get; set; }
        public double LevelIndBm { get; set; }
    }

    public interface IAnalyzerMarker
    {
        Task Start(AnalyzerMarkerConfig config);
        Task Stop();
    }

    public class AnalyzerMarker:IAnalyzerMarker
    {
        private readonly IAnalyzerIq _analyzerIq;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private int _isDisposed;
        private readonly RxValue<MarkerValue> _value = new RxValue<MarkerValue>();
        private AnalyzerMarkerConfig _config;
        private AnalyzerIqInfo _deviceConfig;
        private double[] _window;
        private int _isBusy;
        private Stopwatch _sw;
        private alglib.complex[] _fftArr;
        private double[] _fft;
        private CancellationToken DisposeCancel => _cancel.Token;
        private readonly Subject<double[]> _onFft = new Subject<double[]>();
        private readonly Subject<double[]> _onSignal = new Subject<double[]>();
        private KalmanFilterSimple1D _kalmanAm400;
        private KalmanFilterSimple1D _kalmanAm1300;
        private KalmanFilterSimple1D _kalmanAm3000;
        private KalmanFilterSimple1D _kalmanLevel;

        private LinkedList<double> _am400History = new LinkedList<double>();
        private LinkedList<double> _am1300History = new LinkedList<double>();
        private LinkedList<double> _am3000History = new LinkedList<double>();

        private Thread _thread;

        public AnalyzerMarker(IAnalyzerIq analyzerIq)
        {
            _analyzerIq = analyzerIq ?? throw new ArgumentNullException(nameof(analyzerIq));
            _sw = new Stopwatch();
        }

        public async Task Start(AnalyzerMarkerConfig config)
        {
            await Stop();
            _config = config;
            await _analyzerIq.Open(DisposeCancel);
            var limits = await _analyzerIq.GetLimits(DisposeCancel);
            var sampleRate = limits.SampleRates.OrderBy(_ => Math.Abs(_.IQPairsPerSec - _config.SampleRate)).First();
            await _analyzerIq.SetConfig(new AnalyzerIqConfig
            {
                BandwidthHz = _config.Bandwidth,
                Decimation = sampleRate.Decimation,
                CenterFrequencyHz = _config.FrequencyHz,
            }, DisposeCancel);

            _deviceConfig = await _analyzerIq.GetConfig(DisposeCancel);
            _fftArr = new alglib.complex[_config.FftSize];
            _fft = new double[_config.FftSize];
            _window = WindowFilters.Create(_config.FftWindowFilter, _fft.Length);
            _thread = new Thread(MeasureTick);
            _thread.Start();
        }

        public IObservable<double[]> OnFft => _onFft;
        public IObservable<double[]> OnSignal => _onSignal;

        private async void MeasureTick()
        {
            while (true)
            {
                if (Interlocked.CompareExchange(ref _isBusy, 1, 0) != 0) return;
                try
                {
                    var readCount = _config.FftSize;

                    _sw.Start();

                    var data = await _analyzerIq.Read(new AnalyzerIqRequest { Count = (int)readCount, SkipOldData = false }, DisposeCancel);

                    var readDataTime = _sw.Elapsed;

                    for (int i = 0; i < _config.FftSize; i++)
                    {
                        data.Mag[i] = data.Mag[i] * _window[i];
                        _fftArr[i] = new alglib.complex(data.Mag[i]);
                    }

                    alglib.fftc1d(ref _fftArr);

                    for (int i = 0; i < _config.FftSize; i++)
                    {
                        _fft[i] = Math.Sqrt(_fftArr[i].x * _fftArr[i].x + _fftArr[i].y * _fftArr[i].y);
                    }

                    if (_onSignal.HasObservers)
                    {
                        _onSignal.OnNext((double[])data.Mag.Clone());
                    }

                    if (_onFft.HasObservers)
                    {
                        _onFft.OnNext((double[]) _fft.Clone());
                    }

                    var lvl = _fft[0];
                    var lvl400 = _fft[(int)Math.Round(400.0 / _deviceConfig.SampleRate.IQPairsPerSec * readCount)];
                    var lvl1300 = _fft[(int)Math.Round((1300 / _deviceConfig.SampleRate.IQPairsPerSec * readCount))];
                    var lvl3000 = _fft[(int)Math.Round(3000 / _deviceConfig.SampleRate.IQPairsPerSec * readCount)];

                    var am400 = 100.0 * lvl400 / lvl * 2;
                    var am1300 = 100.0 * lvl1300 / lvl * 2;
                    var am3000 = 100.0 * lvl3000 / lvl * 2;

                    _am400History.AddLast(am400);
                    while (_am400History.Count > _config.AmHistorySize)
                    {
                        _am400History.RemoveFirst();
                    }

                    _am1300History.AddLast(am1300);
                    while (_am1300History.Count > _config.AmHistorySize)
                    {
                        _am1300History.RemoveFirst();
                    }
                    _am3000History.AddLast(am3000);
                    while (_am3000History.Count > _config.AmHistorySize)
                    {
                        _am3000History.RemoveFirst();
                    }

                    am400 = _am400History.Max();
                    am1300 = _am1300History.Max();
                    am3000 = _am3000History.Max();

                    if (_kalmanAm400 == null)
                    {
                        _kalmanAm400 = new KalmanFilterSimple1D(_config.AmKalman.MeasurementNoise, 1, 1, 1);
                        _kalmanAm400.SetState(am400, _config.AmKalman.MeasurementNoise);
                    }
                    _kalmanAm400.Correct(am400);
                    if (Math.Abs(_kalmanAm400.State - am400) > _config.AmKalman.ResetFilterCondition)
                    {
                        _kalmanAm400 = new KalmanFilterSimple1D(_config.AmKalman.MeasurementNoise, 1, 1, 1);
                        _kalmanAm400.SetState(am400, _config.AmKalman.MeasurementNoise);
                    }
                    
                    
                    if (_kalmanAm1300 == null)
                    {
                        _kalmanAm1300 = new KalmanFilterSimple1D(_config.AmKalman.MeasurementNoise, 1, 1, 1);
                        _kalmanAm1300.SetState(am1300, _config.AmKalman.MeasurementNoise);
                    }
                    _kalmanAm1300.Correct(am1300);
                    if (Math.Abs(_kalmanAm1300.State - am1300) > _config.AmKalman.ResetFilterCondition)
                    {
                        _kalmanAm1300 = new KalmanFilterSimple1D(_config.AmKalman.MeasurementNoise, 1, 1, 1);
                        _kalmanAm1300.SetState(am1300, _config.AmKalman.MeasurementNoise);
                    }
                    
                    if (_kalmanAm3000 == null)
                    {
                        _kalmanAm3000 = new KalmanFilterSimple1D(_config.AmKalman.MeasurementNoise, 1, 1, 1);
                        _kalmanAm3000.SetState(am3000, _config.AmKalman.MeasurementNoise);
                    }
                    _kalmanAm3000.Correct(am3000);
                    if (Math.Abs(_kalmanAm3000.State - am3000) > _config.AmKalman.ResetFilterCondition)
                    {
                        _kalmanAm3000 = new KalmanFilterSimple1D(_config.AmKalman.MeasurementNoise, 1, 1, 1);
                        _kalmanAm3000.SetState(am3000, _config.AmKalman.MeasurementNoise);
                    }


                    if (_kalmanLevel == null)
                    {
                        _kalmanLevel = new KalmanFilterSimple1D(_config.LevelKalman.MeasurementNoise, 1, 1, 1);
                        _kalmanLevel.SetState(data.LevelIndBm, _config.LevelKalman.MeasurementNoise);
                    }
                    _kalmanLevel.Correct(data.LevelIndBm);
                    if (Math.Abs(_kalmanLevel.State - data.LevelIndBm) > _config.LevelKalman.ResetFilterCondition)
                    {
                        _kalmanLevel = new KalmanFilterSimple1D(_config.LevelKalman.MeasurementNoise, 1, 1, 1);
                        _kalmanLevel.SetState(data.LevelIndBm, _config.LevelKalman.MeasurementNoise);
                    }

                    _sw.Stop();
                    
                    _value.OnNext(new MarkerValue
                    {
                        Am400 = _kalmanAm400.State,
                        Am1300 = _kalmanAm1300.State,
                        Am3000 = _kalmanAm3000.State,
                        LevelIndBm = _kalmanLevel.State,
                        FreqHz = _config.FrequencyHz,
                        ReadDataTime = readDataTime,
                        AllTime = _sw.Elapsed,
                    });

                }
                finally
                {
                    Interlocked.Exchange(ref _isBusy, 0);
                }
            }
            
        }

        public async Task Stop()
        {
            try
            {
                _thread?.Abort();
            }
            catch (Exception e)
            {
                
            }
            
            await _analyzerIq.Close(DisposeCancel);
        }

        public IRxValue<MarkerValue> Value => _value;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            Stop().Wait();
            _cancel.Cancel(false);
            _cancel.Dispose();
        }

    }
}