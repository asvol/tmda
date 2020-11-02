using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Tmda.Core
{
    public interface IAnalyzerIqProvider
    {
        string TypeName { get; }
        Task<AnalyzerIqDeviceInfo[]> GetDevices(CancellationToken cancel);
        Task<IAnalyzerIq> Create(string id, CancellationToken cancel);
    }

    public class AnalyzerIqDeviceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }


    public class AnalyzerIqDevice
    {
        public string Id { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
    }

    public interface IAnalyzerIqFactory
    {
        Task<AnalyzerIqDevice[]> GetDevices(CancellationToken cancel);
        Task<IAnalyzerIq> Create(AnalyzerIqDevice device, CancellationToken cancel);
    }

    [Export(typeof(IAnalyzerIqFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AnalyzerIqFactory : IAnalyzerIqFactory
    {
        private readonly IEnumerable<IAnalyzerIqProvider> _providers;

        [ImportingConstructor]
        public AnalyzerIqFactory([ImportMany]IEnumerable<IAnalyzerIqProvider> providers)
        {
            _providers = providers;
        }

        public async Task<AnalyzerIqDevice[]> GetDevices(CancellationToken cancel)
        {
            var items = _providers.Select(_ => new {task = _.GetDevices(cancel), Name = _.TypeName});
            await Task.WhenAll(items.Select(_=>_.task));
            var result = new List<AnalyzerIqDevice>();
            foreach (var item in items)
            {
                foreach (var deviceInfo in item.task.Result)
                {
                    result.Add(new AnalyzerIqDevice
                    {
                        Id = deviceInfo.Id,
                        Name = deviceInfo.Name,
                        TypeName = item.Name,
                    });
                }
            }
            return result.ToArray();
        }

        public Task<IAnalyzerIq> Create(AnalyzerIqDevice device, CancellationToken cancel)
        {
            var provider = _providers.FirstOrDefault(_ => _.TypeName == device.TypeName);
            if (provider == null) throw new Exception($"Device provider '{device.TypeName}' not found");
            return provider.Create(device.Id, cancel);
        }
    }
}