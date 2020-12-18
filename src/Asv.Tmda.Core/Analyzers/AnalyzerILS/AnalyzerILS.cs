using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Tmda.Core.ILS
{
   

    public class AnalyzerILS: IAnalyzerILS
    {
        private readonly IAnalyzerIq _analyzerIq;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private int _isDisposed;
        
        private int _isBusy;
        private AnalyzerILSConfig _config;
        private AnalyzerIqInfo _deviceConfig;
        private double[] _window;
        private readonly RxValue<IlsValue[]> _value = new RxValue<IlsValue[]>();
        private AnalyzerILSFreq[] _freq;
        private Thread _thread;
        private CancellationToken DisposeCancel => _cancel.Token;

        public AnalyzerILS(IAnalyzerIq analyzerIq)
        {
            _analyzerIq = analyzerIq ?? throw new ArgumentNullException(nameof(analyzerIq));
        }

        public async Task Start(AnalyzerILSConfig config)
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
                CenterFrequencyHz = _config.Frequencies.First()
            }, DisposeCancel);
            _deviceConfig = await _analyzerIq.GetConfig(DisposeCancel);

            var measurePerSecond = _config.MeasurePerSecond * _config.Frequencies.Count;
            var readSamples = ((int)(_deviceConfig.SampleRate.IQPairsPerSec / measurePerSecond / 30)) * 30;
            var thinningPoints = readSamples / _config.FftSize;
            var fftSize = readSamples / thinningPoints;
            readSamples += thinningPoints;
            _window = WindowFilters.Create(_config.FftWindowFilter, fftSize);
            var changeFreq = _config.Frequencies.Count != 1;
            _freq = _config.Frequencies.Select(_ => new AnalyzerILSFreq(_, _config, measurePerSecond, _analyzerIq, _deviceConfig, _window, DisposeCancel, readSamples, thinningPoints, fftSize, changeFreq)).ToArray();
            _thread = new Thread(MeasureTick);
            _thread.Start();
        }

        private async void MeasureTick()
        {
            while (true)
            {
                if (Interlocked.CompareExchange(ref _isBusy, 1, 0) != 0) return;
                try
                {
                    var values = new IlsValue[_freq.Length];
                    for (var i = 0; i < _freq.Length; i++)
                    {
                        values[i] = await _freq[i].Calculate();
                    }

                    _value.OnNext(values);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    Interlocked.Exchange(ref _isBusy, 0);
                }
            }
        }

        public IObservable<double[]> OnFft(double freq)
        {
            return _freq.First(_ => Math.Abs(_.Freq - freq) < double.Epsilon).OnFft;
        }

        public IObservable<double[]> OnSignal(double freq)
        {
            var a = _freq.First(_ => Math.Abs(_.Freq - freq) < double.Epsilon);
            return a.OnSignal;
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

        public IRxValue<IlsValue[]> Value => _value;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            Stop().Wait();
            _cancel.Cancel(false);
            _cancel.Dispose();
        }

    }

    public class AnalyzerILSFreq
    {
        private readonly double _freqHz;
        private readonly AnalyzerILSConfig _config;
        private readonly int _measurePerSecond;
        private readonly IAnalyzerIq _analyzerIq;
        private readonly double[] _window;
        private readonly CancellationToken _cancel;
        private KalmanFilterSimple1D _kalmanDDM;
        private KalmanFilterSimple1D _kalmanSDM;
        private readonly int _readSamples;
        private readonly int _thinningPoints;
        private readonly int _fftSize;
        private readonly bool _changeFreq;
        private alglib.complex[] _fftArr;
        private readonly IlsValue[] _resultArray;
        private readonly double[] _fft;
        private Stopwatch _sw;
        private readonly Subject<double[]> _onFft = new Subject<double[]>();
        private readonly Subject<double[]> _onSignal = new Subject<double[]>();

        public AnalyzerILSFreq(double freqHz, AnalyzerILSConfig config, int measurePerSecond, IAnalyzerIq analyzerIq, AnalyzerIqInfo deviceConfig, double[] window, CancellationToken cancel, int readSamples, int thinningPoints, int fftSize, bool changeFreq)
        {
            _freqHz = freqHz;
            _config = config;
            _measurePerSecond = measurePerSecond;

            _analyzerIq = analyzerIq;
            _window = window;
            _cancel = cancel;
            _readSamples = readSamples;
            _thinningPoints = thinningPoints;
            _fftSize = fftSize;
            _changeFreq = changeFreq;
            _resultArray = new IlsValue[_thinningPoints];
            for (int i = 0; i < _thinningPoints; i++)
            {
                _resultArray[i] = new IlsValue();
            }
            _fftArr = new alglib.complex[_fftSize];
            _fft = new double[_fftSize];
            _sw = new Stopwatch();
        }

        public IObservable<double[]> OnFft => _onFft;
        public IObservable<double[]> OnSignal => _onSignal;
        public double Freq => _freqHz;

        public async Task<IlsValue> Calculate()
        {
            _sw.Reset();
            _sw.Start();
            if (_changeFreq) await _analyzerIq.SetFreq(_freqHz, CancellationToken.None);
            var data = await _analyzerIq.Read(new AnalyzerIqRequest { Count = _readSamples, SkipOldData = true }, _cancel);
            
            var readDataTime = _sw.Elapsed;

            for (int i = 0; i < _thinningPoints; i++)
            {
                for (int j = 0; j < _fftSize; j++)
                {
                    _fftArr[j] = new alglib.complex(data.Mag[(j*_thinningPoints + i)] * _window[j],0);
                }

                if (_onSignal.HasObservers)
                {
                    _onSignal.OnNext(_fftArr.Select(_ => _.x).ToArray());
                }

                alglib.fftc1d(ref _fftArr);
                for (var index = 0; index < _fftArr.Length; index++)
                {
                    _fft[index] = Math.Sqrt(_fftArr[index].x * _fftArr[index].x + _fftArr[index].y * _fftArr[index].y);
                }
                var lvl = _fft[0];
                var lvl90 = _fft[90 / _measurePerSecond];
                var lvl150 = _fft[150 / _measurePerSecond];
                _resultArray[i].DDM = 100.0 * 2.0 * (lvl150 - lvl90) / lvl;
                _resultArray[i].SDM = 100.0 * 2.0 * (lvl150 + lvl90) / lvl;

               
                if (_onFft.HasObservers)
                {
                    _onFft.OnNext((double[])_fft.Clone());
                }
            }

            var mean = _resultArray.OrderBy(_ => _.SDM).Skip(_resultArray.Length/2).First();

            
            if (_kalmanDDM == null)
            {
                _kalmanDDM = new KalmanFilterSimple1D(_config.KalmanDDM.MeasurementNoise, 1, 1, 1);
                _kalmanDDM.SetState(mean.DDM, _config.KalmanDDM.MeasurementNoise);
            }
            _kalmanDDM.Correct(mean.DDM);
            if (Math.Abs(_kalmanDDM.State - mean.DDM) > _config.KalmanDDM.ResetFilterCondition)
            {
                _kalmanDDM = new KalmanFilterSimple1D(_config.KalmanDDM.MeasurementNoise, 1, 1, 1);
                _kalmanDDM.SetState(mean.DDM, _config.KalmanDDM.MeasurementNoise);
            }

            if (_kalmanSDM == null)
            {
                _kalmanSDM = new KalmanFilterSimple1D(_config.KalmanSDM.MeasurementNoise, 1, 1, 1);
                _kalmanSDM.SetState(mean.SDM, _config.KalmanSDM.MeasurementNoise);
            }
            _kalmanSDM.Correct(mean.SDM);

            if (Math.Abs(_kalmanSDM.State - mean.SDM) > _config.KalmanSDM.ResetFilterCondition)
            {
                _kalmanSDM = new KalmanFilterSimple1D(_config.KalmanSDM.MeasurementNoise, 1, 1, 1);
                _kalmanSDM.SetState(mean.SDM, _config.KalmanSDM.MeasurementNoise);
            }
            _sw.Stop();

            return new IlsValue
            {
                DDM = _kalmanDDM.State,
                SDM = _kalmanSDM.State,
                LevelIndBm = data.LevelIndBm,
                LevelInmW = data.LevelInmW,
                FreqHz = _freqHz,
                ReadDataTime = readDataTime,
                AllTime = _sw.Elapsed,
            };
        }
    }
}