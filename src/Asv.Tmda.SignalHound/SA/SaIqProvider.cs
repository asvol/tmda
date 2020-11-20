using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tmda.Core;
using NLog;

namespace Asv.Tmda.SignalHound
{
    [Export(typeof(IDeviceProvider<IAnalyzerIq>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SaIqProvider : IDeviceProvider<IAnalyzerIq>
    {
        private readonly object _sync = new object();
        public string TypeName => "SignalHound SA Series";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [ImportingConstructor]
        public SaIqProvider()
        {
            LibHelper.CheckLibraryFiles();
        }

        public Task<IReadOnlyList<ProviderDeviceInfo>> GetDevices(CancellationToken cancel)
        {
            return Task.Factory.StartNew(() =>
            {
                lock (_sync)
                {
                    var serials = new int[8];
                    var deviceCount = 0;
                    InternalCheckStatus(sa_api.saGetSerialNumberList(serials, ref deviceCount));
                    var result = new List<ProviderDeviceInfo>();
                    for (var i = 0; i < serials.Length; i++)
                    {
                        if (serials[i] <= 0) continue;
                        result.Add(new ProviderDeviceInfo
                        {
                            Id = serials[i].ToString(),
                            Name = $"{TypeName} SN:{serials[i]}",
                        });
                    }
                    return (IReadOnlyList<ProviderDeviceInfo>)result;
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