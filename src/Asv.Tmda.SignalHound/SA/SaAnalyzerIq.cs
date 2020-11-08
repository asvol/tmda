using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Asv.Tmda.Core;
using NLog;

namespace Asv.Tmda.SignalHound
{
    public class SaAnalyzerIq : ThreadSafeAnalyzerIqBase
    {
        #region static

        private static AnalyzerIqLimits _limits;

        static SaAnalyzerIq()
        {
            _limits = new AnalyzerIqLimits
            {
                Freq = new Limits<double> { Max = 4_400_000_000, Min = 1 },
                SampleRates = new SampleRateLimit[]
                {
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 250_000, Min = 0},
                        Decimation = 1,
                        IQPairsPerSec = 486_111.11111,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 225_000, Min = 0},
                        Decimation = 2,
                        IQPairsPerSec = 486_111.11111/2,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 100_000, Min = 0},
                        Decimation = 4,
                        IQPairsPerSec = 486_111.11111/4,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 50_000, Min = 0},
                        Decimation = 8,
                        IQPairsPerSec = 486_111.11111/8,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 20_000, Min = 0},
                        Decimation = 16,
                        IQPairsPerSec = 486_111.11111/16,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 12_000, Min = 0},
                        Decimation = 32,
                        IQPairsPerSec = 486_111.11111/32,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 5_000, Min = 0},
                        Decimation = 64,
                        IQPairsPerSec = 486_111.11111/64,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 3_000, Min = 0},
                        Decimation = 128,
                        IQPairsPerSec = 486_111.11111/128,
                    },
                },
            };


        }

        #endregion

        private readonly int _serialNumber;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private int _deviceHandle;
        private double _centerFreqHz;
        private double _bandwidth;
        private SampleRateLimit _sampleRate;
        private double _refLevel;

        public SaAnalyzerIq(int serialNumber)
        {
            _serialNumber = serialNumber;
        }

        protected override void InternalOpen()
        {
            _logger.Info($"Open device SN {_serialNumber}");
            InternalCheckStatus(sa_api.saOpenDeviceBySerialNumber(ref _deviceHandle, _serialNumber));
            _logger.Info("Serial Number: " + bb_api.bbGetSerialString(_deviceHandle));
            _logger.Info("Firmware Version: " + bb_api.bbGetFirmwareString(_deviceHandle));
        }

        protected override void InternalClose()
        {
            InternalCheckStatus(sa_api.saCloseDevice(_deviceHandle));
        }

        protected override AnalyzerIqLimits InternalGetLimits()
        {
            return _limits;
        }

        protected override void InternalSetFreq(double freqHz)
        {
            InternalCheckStatus(sa_api.saConfigCenterSpan(_deviceHandle, freqHz, 350));
            InternalCheckStatus(sa_api.saInitiate(_deviceHandle, sa_api.SA_IQ, 0));
            Thread.Sleep(180);
        }

        protected override void InternalSetConfig(AnalyzerIqConfig cfg)
        {
            _centerFreqHz = cfg.CenterFrequencyHz;

            _sampleRate = _limits.SampleRates.FirstOrDefault(_ => _.Decimation == cfg.Decimation);
            if (_sampleRate == null) throw new Exception($"Wrong decimation value {cfg.Decimation}. Available values: {string.Join(",", _limits.SampleRates.Select(_ => _.Decimation))}");
            _refLevel = -40;
            InternalCheckStatus(sa_api.saConfigCenterSpan(_deviceHandle, cfg.CenterFrequencyHz, 350));
            InternalCheckStatus(sa_api.saConfigLevel(_deviceHandle, _refLevel));
            InternalCheckStatus(sa_api.saConfigGainAtten(_deviceHandle, sa_api.SA_AUTO_ATTEN, sa_api.SA_AUTO_GAIN, true));
            InternalCheckStatus(sa_api.saConfigIQ(_deviceHandle, cfg.Decimation, cfg.BandwidthHz));
            InternalCheckStatus(sa_api.saInitiate(_deviceHandle, sa_api.SA_IQ, 0));
            var unused = 0;
            _bandwidth = 0.0;
            var samplesPerSec = 0.0;
            InternalCheckStatus(sa_api.saQueryStreamInfo(_deviceHandle, ref unused, ref _bandwidth, ref samplesPerSec));
            Debug.Assert(Math.Abs(_sampleRate.IQPairsPerSec - samplesPerSec) > double.Epsilon);
        }

        protected override AnalyzerIqInfo InternalGetConfig()
        {
            return new AnalyzerIqInfo
            {
                Bandwidth = _bandwidth,
                CenterFrequencyHz = _centerFreqHz,
                SampleRate = _sampleRate,
                RefLevel = _refLevel,
            };
        }

        private void CalculateRefLevel(double currentLevel)
        {
            _refLevel = currentLevel - 5;
            var sw = new Stopwatch();
            sw.Start();
            sa_api.saConfigLevel(_deviceHandle, _refLevel);
            sa_api.saInitiate(_deviceHandle, sa_api.SA_IQ, 0);
            sw.Stop();
            Debug.WriteLine($"Found new ref level:{_refLevel} by {sw.Elapsed.TotalMilliseconds:F0} ms");
            _logger.Info($"Found new ref level:{_refLevel} by {sw.Elapsed.TotalMilliseconds:F0} ms");
            Thread.Sleep(180);

        }

        protected override AnalyzerIqPacket InternalRead(AnalyzerIqRequest query)
        {
            var iqSamples = new float[query.Count * 2];
            double[] mag = new double[query.Count];
            int triggerLen = 16;
            var triggers = new int[triggerLen];
            int dataRemaining = 0, sampleLoss = 0, iqSec = 0, iqNano = 0;
            var sw = new Stopwatch();

            start:
            sw.Start();
            var status = sa_api.saGetIQDataUnpacked(_deviceHandle, iqSamples, query.Count, query.SkipOldData ? 1 : 0, ref dataRemaining, ref sampleLoss, ref iqSec, ref iqNano);

            InternalCheckStatus(status);
            double summ = 0;
            for (int i = 0; i < iqSamples.Length; i += 2)
            {
                mag[i / 2] = Math.Sqrt(iqSamples[i] * iqSamples[i] + iqSamples[i + 1] * iqSamples[i + 1]);
                summ += mag[i / 2];
            }
            var levelmW = summ / mag.Length;
            var leveldBm = 10.0 * Math.Log10(levelmW * levelmW);

            if (Math.Abs(leveldBm - _refLevel) > 7)
            {
                CalculateRefLevel(leveldBm);
                goto start;
            }
            
            sw.Stop();
            return new AnalyzerIqPacket
            {
                IqSamples = iqSamples,
                Mag = mag,
                LevelInmW = levelmW,
                LevelIndBm = leveldBm,
                DataRemaining = dataRemaining,
                SampleLoss = sampleLoss,
                ElapsedTime = sw.Elapsed,
                CenterFrequencyHz = _centerFreqHz,
            };
        }

        protected override void InternalDisposeOnce()
        {
            
        }

        private void InternalCheckStatus(saStatus status)
        {
            if (status == saStatus.saNoError) return;
            if (status == saStatus.saBandwidthClamped) return;
            if (status == saStatus.saParameterClamped) return;

            var err = sa_api.saGetStatusString(status);
            _logger.Error($"SignalHound device error: {err}");
            throw new Exception(err);
        }
    }
}