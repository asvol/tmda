using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Tmda.Core
{
    public class DeviceFactoryBase<TDevice> : IDeviceFactory<TDevice>
    {
        private readonly IEnumerable<IDeviceProvider<TDevice>> _providers;

        public DeviceFactoryBase(IEnumerable<IDeviceProvider<TDevice>> providers)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
            foreach (var group in _providers.GroupBy(_ => _.TypeName))
            {
                if (group.Count() != 1) throw new Exception($"Provider type name not unique: {group.First()} and {group.Skip(1).First()} has same type name '{group.Key}'");
            }

        }

        public async Task<IReadOnlyList<DeviceInfo>> GetDevices(CancellationToken cancel = default)
        {
            var items = _providers.Select(_ => new { task = _.GetDevices(cancel), Name = _.TypeName });
            await Task.WhenAll(items.Select(_ => _.task));
            var result = new List<DeviceInfo>();
            foreach (var item in items)
            {
                foreach (var deviceInfo in item.task.Result)
                {
                    result.Add(new DeviceInfo
                    {
                        Id = deviceInfo.Id,
                        Name = deviceInfo.Name,
                        TypeName = item.Name,
                    });
                }
            }
            return result;
        }

        public Task<TDevice> Create(DeviceInfo device, CancellationToken cancel = default)
        {
            var provider = _providers.FirstOrDefault(_ => _.TypeName == device.TypeName);
            if (provider == null) throw new Exception($"Device provider '{device.TypeName}' not found");
            return provider.Create(device.Id, cancel);
        }
    }
}