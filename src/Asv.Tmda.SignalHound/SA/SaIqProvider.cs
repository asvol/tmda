using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tmda.Core;
using NLog;

namespace Asv.Tmda.SignalHound
{
    [Export(typeof(IAnalyzerIqProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SaIqProvider : IAnalyzerIqProvider
    {
        private readonly object _sync = new object();
        public string TypeName => "SignalHound SA Series";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [ImportingConstructor]
        public SaIqProvider()
        {
            LibHelper.CheckLibraryFiles();
        }

        public Task<AnalyzerIqDeviceInfo[]> GetDevices(CancellationToken cancel)
        {
            return Task.Factory.StartNew(() =>
            {
                lock (_sync)
                {
                    var serials = new int[8];
                    var deviceCount = 0;
                    InternalCheckStatus(sa_api.saGetSerialNumberList(serials, ref deviceCount));
                    var result = new AnalyzerIqDeviceInfo[serials.Count(_ => _ > 0)];
                    for (var i = 0; i < serials.Length; i++)
                    {
                        if (serials[i] <= 0) continue;
                        result[i] = new AnalyzerIqDeviceInfo
                        {
                            Id = serials[i].ToString(),
                            Name = $"{TypeName} SN:{serials[i]}",
                        };
                    }
                    return result;
                }
            }, cancel);
        }

        private void InternalCheckStatus(saStatus status)
        {
            if (status == saStatus.saNoError) return;
            var err = sa_api.saGetStatusString(status);
            Logger.Error($"SignalHound SA series device error: {err}");
            throw new Exception(err);
        }

        public Task<IAnalyzerIq> Create(string id, CancellationToken cancel)
        {
            return Task.FromResult<IAnalyzerIq>(new SaAnalyzerIq(int.Parse(id)));
        }
    }
}