﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using Asv.Tmda.Core;
using NLog;
using NLog.Targets;

namespace Asv.Tmda.SignalHound
{

    

    public class BbAnalyzerIq:ThreadSafeAnalyzerIqBase
    {
        #region static

        private static AnalyzerIqLimits _limits;

        static BbAnalyzerIq()
        {
            _limits = new AnalyzerIqLimits
            {
                Freq = new Limits<double> {Max = bb_api.BB60_MAX_FREQ, Min = bb_api.BB60_MIN_FREQ},
                SampleRates = new SampleRateLimit[]
                {
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 27_000_000, Min = 0},
                        Decimation = 1,
                        IQPairsPerSec = 40_000_000,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 17_800_000, Min = 0},
                        Decimation = 2,
                        IQPairsPerSec = 20_000_000,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 8_000_000, Min = 0},
                        Decimation = 4,
                        IQPairsPerSec = 10_000_000,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 3_750_000, Min = 0},
                        Decimation = 8,
                        IQPairsPerSec = 5_000_000,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 2_000_000, Min = 0},
                        Decimation = 16,
                        IQPairsPerSec = 2_500_000,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 1_000_000, Min = 0},
                        Decimation = 32,
                        IQPairsPerSec = 1_250_000,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 500_000, Min = 0},
                        Decimation = 64,
                        IQPairsPerSec = 625_000,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 250_000, Min = 0},
                        Decimation = 128,
                        IQPairsPerSec = 312_500,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 140_000, Min = 0},
                        Decimation = 256,
                        IQPairsPerSec = 156_250,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 65_000, Min = 10_000},
                        Decimation = 512,
                        IQPairsPerSec = 78_0125,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 30_000, Min = 5_000},
                        Decimation = 1024,
                        IQPairsPerSec = 39062.5,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 15_000, Min = 0},
                        Decimation = 2048,
                        IQPairsPerSec = 19531.25,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 8_000, Min = 0},
                        Decimation = 4096,
                        IQPairsPerSec = 9765.625,
                    },
                    new SampleRateLimit
                    {
                        Bandwidth = new Limits<double> {Max = 4_000, Min = 0},
                        Decimation = 8192,
                        IQPairsPerSec = 4882.8125,
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
        private double _prevLevel;

        public BbAnalyzerIq(int serialNumber)
        {
            _serialNumber = serialNumber;
        }

        protected override void InternalOpen()
        {
            _logger.Info($"Open device SN {_serialNumber}");
            InternalCheckStatus(bb_api.bbOpenDeviceBySerialNumber(ref _deviceHandle, _serialNumber));
            _logger.Info("API Version: " + bb_api.bbGetDeviceName(_deviceHandle));
            _logger.Info("Serial Number: " + bb_api.bbGetSerialString(_deviceHandle));
            _logger.Info("Firmware Version: " + bb_api.bbGetFirmwareString(_deviceHandle));
        }

        protected override void InternalClose()
        {
            var status = bb_api.bbCloseDevice(_deviceHandle);
            //if (status == bbStatus.bbDeviceNotOpenErr) return;
            InternalCheckStatus(status);
        }

        protected override AnalyzerIqLimits InternalGetLimits()
        {
            return _limits;
        }

        protected override void InternalSetFreq(double freqHz)
        {
            InternalCheckStatus(bb_api.bbConfigureCenterSpan(_deviceHandle, freqHz, 350));
            InternalCheckStatus(bb_api.bbInitiate(_deviceHandle, bb_api.BB_STREAMING, bb_api.BB_STREAM_IQ));
            Thread.Sleep(50);
        }

        protected override void InternalSetConfig(AnalyzerIqConfig cfg)
        {
            _centerFreqHz = cfg.CenterFrequencyHz;

            _sampleRate =  _limits.SampleRates.FirstOrDefault(_ => _.Decimation == cfg.Decimation);
            if (_sampleRate == null) throw new Exception($"Wrong decimation value {cfg.Decimation}. Available values: {string.Join(",", _limits.SampleRates.Select(_=>_.Decimation))}");
            _refLevel = -130;
            InternalCheckStatus(bb_api.bbConfigureCenterSpan(_deviceHandle, cfg.CenterFrequencyHz, 350));
            InternalCheckStatus(bb_api.bbConfigureLevel(_deviceHandle, _refLevel, bb_api.BB_AUTO_ATTEN));
            InternalCheckStatus(bb_api.bbConfigureGain(_deviceHandle, bb_api.BB_AUTO_GAIN));
            InternalCheckStatus(bb_api.bbConfigureIQ(_deviceHandle, cfg.Decimation, cfg.BandwidthHz));
            InternalCheckStatus(bb_api.bbInitiate(_deviceHandle, bb_api.BB_STREAMING, bb_api.BB_STREAM_IQ));
            var unused = 0;
            _bandwidth = 0.0;
            var samplesPerSec = 0;
            InternalCheckStatus(bb_api.bbQueryStreamInfo(_deviceHandle, ref unused, ref _bandwidth, ref samplesPerSec));
            //Debug.Assert(Math.Abs(_sampleRate.IQPairsPerSec - samplesPerSec) > double.Epsilon);
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

        private void CalculateRefLevel()
        {
            bbStatus status;
            var refLevel = -40;
            var iqSamples = new float[10];
            int triggerLen = 16;
            var triggers = new int[triggerLen];
            int dataRemaining = 0, sampleLoss = 0, iqSec = 0, iqNano = 0;
            var sw = new Stopwatch();
            sw.Start();
            do
            {
                bb_api.bbConfigureLevel(_deviceHandle, refLevel, bb_api.BB_AUTO_ATTEN);
                bb_api.bbInitiate(_deviceHandle, bb_api.BB_STREAMING, bb_api.BB_STREAM_IQ);
                status = bb_api.bbGetIQUnpacked(_deviceHandle, iqSamples, iqSamples.Length/2, triggers, triggerLen, 1, ref dataRemaining, ref sampleLoss, ref iqSec, ref iqNano);
                refLevel += 5;
            } while (status == bbStatus.bbADCOverflow);

            _refLevel = refLevel - 5;

            bb_api.bbConfigureLevel(_deviceHandle, _refLevel, bb_api.BB_AUTO_ATTEN);
            bb_api.bbInitiate(_deviceHandle, bb_api.BB_STREAMING, bb_api.BB_STREAM_IQ);
            sw.Stop();
            Debug.WriteLine($"Found new ref level:{_refLevel} by {sw.Elapsed.TotalMilliseconds:F0} ms");
            _logger.Info($"Found new ref level:{_refLevel} by {sw.Elapsed.TotalMilliseconds:F0} ms");
            Thread.Sleep(50);

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
            var status = bb_api.bbGetIQUnpacked(_deviceHandle, iqSamples, query.Count, triggers, triggerLen,  query.SkipOldData ? 1 : 0, ref dataRemaining, ref sampleLoss, ref iqSec, ref iqNano);
            
            
            if (status == bbStatus.bbADCOverflow)
            {
                Console.WriteLine("ADC overflow");
                CalculateRefLevel();
                goto start;
            }
            InternalCheckStatus(status);
            double summ = 0;
            for (int i = 0; i < iqSamples.Length; i+=2)
            {
                mag[i/2] = Math.Sqrt(iqSamples[i] * iqSamples[i] + iqSamples[i + 1] * iqSamples[i + 1]);
                summ += mag[i/2];
            }
            var levelmW = summ / mag.Length;
            var leveldBm = 10.0 * Math.Log10(levelmW * levelmW);
            
            if (Math.Abs(_prevLevel - leveldBm) > 20 )
            {
               CalculateRefLevel();
               Console.WriteLine("Level chaged");
                _prevLevel = leveldBm;
               goto start;
            }
            sw.Stop();
            return new AnalyzerIqPacket
            {
                IqSamples = iqSamples,
                Mag = mag,
                LevelInmW = leveldBm,
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

        private void InternalCheckStatus(bbStatus status)
        {
            if (status == bbStatus.bbNoError) return;
            if (status == bbStatus.bbClampedToLowerLimit) return;
            if (status == bbStatus.bbClampedToUpperLimit) return;

            var err = bb_api.bbGetStatusString(status);
            _logger.Error($"SignalHound device error: {err}");
            throw new Exception(err);
        }
    }

    
}