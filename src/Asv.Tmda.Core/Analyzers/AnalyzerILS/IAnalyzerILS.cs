using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Asv.Tmda.Core.ILS
{
    public interface IAnalyzerILS:IDisposable
    {
        Task Start(AnalyzerILSConfig config);
        Task Stop();
        IRxValue<IlsValue[]> Value { get; }
        IObservable<double[]> OnFft(double freq);
        IObservable<double[]> OnSignal(double freq);
    }

    public class IlsValue
    {
        public double DDM { get; set; }
        public double SDM { get; set; }
        public double LevelIndBm { get; set; }
        public double LevelInmW { get; set; }
        public double FreqHz { get; set; }
        public TimeSpan ReadDataTime { get; set; }
        public TimeSpan AllTime { get; set; }
    }

    

    public class KalmanFilterConfig
    {
        public double MeasurementNoise { get; set; } = 0.03;
        public double ResetFilterCondition { get; set; } = 3;
    }

    public class AnalyzerILSConfig
    {
        public double Bandwidth { get; set; } = 5_000;
        public double SampleRate { get; set; } = 40_000;
        public WindowFilterEnum FftWindowFilter { get; set; } = WindowFilterEnum.Hamming;
        public int FftSize { get; set; } = 10000;
        public int MeasurePerSecond { get; set; } = 4;
        public List<double> Frequencies { get; } = new List<double>();
        public KalmanFilterConfig KalmanDDM { get; } = new KalmanFilterConfig();
        public KalmanFilterConfig KalmanSDM { get; } = new KalmanFilterConfig();
        
        
    }
}