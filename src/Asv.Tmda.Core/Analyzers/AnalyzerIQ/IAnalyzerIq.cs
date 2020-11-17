using System;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Tmda.Core
{


  

    public class AnalyzerIqConfig
    {
        public double CenterFrequencyHz { get; set; }
        public double BandwidthHz { get; set; }
        public int Decimation { get; set; }
    }

    public class AnalyzerIqInfo
    {
        public double Bandwidth { get; set; }
        public double CenterFrequencyHz { get; set; }
        public SampleRateLimit SampleRate { get; set; }
        public double RefLevel { get; set; }
    }

    public class Limits<T>
    {
        public T Max { get; set; }
        public T Min { get; set; }
    }

    public class SampleRateLimit
    {
        public int Decimation { get; set; }
        public double IQPairsPerSec { get; set; }
        public Limits<double> Bandwidth { get; set; }
    }

    public class AnalyzerIqLimits
    {
       

        public Limits<double> Freq { get; set; }
        public SampleRateLimit[] SampleRates { get; set; }
    }

    public class AnalyzerIqRequest
    {
        public bool SkipOldData { get; set; } = true;
        public int Count { get; set; }
    }

    public class AnalyzerIqPacket
    {
        public float[] IqSamples { get; set; }
        public double[] Mag { get; set; }
        public int DataRemaining { get; set; }
        public int SampleLoss { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public double CenterFrequencyHz { get; set; }
        public double LevelInmW { get; set; }
        public double LevelIndBm { get; set; }
    }

    public interface IAnalyzerIq:IDisposable
    {
        bool IsOpened { get; }
        Task Open(CancellationToken cancel);
        Task Close(CancellationToken cancel);
        Task<AnalyzerIqLimits> GetLimits(CancellationToken cancel);
        Task SetConfig(AnalyzerIqConfig cfg, CancellationToken cancel);
        Task SetFreq(double freqHz, CancellationToken cancel);
        Task<AnalyzerIqInfo> GetConfig(CancellationToken cancel);
        Task<AnalyzerIqPacket> Read(AnalyzerIqRequest query, CancellationToken cancel);
    }
}