using System;
using System.Threading.Tasks;
using NLog;

namespace Asv.Tmda.Core.Marker
{
    public interface IAnalyzerMarker
    {

    }

    public class AnalyzerMarker:IAnalyzerMarker
    {
        private readonly IAnalyzerIq _analyzerIq;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public AnalyzerMarker(IAnalyzerIq analyzerIq)
        {
            _analyzerIq = analyzerIq;
            _analyzerIq = analyzerIq ?? throw new ArgumentNullException(nameof(analyzerIq));
        }
    }
}