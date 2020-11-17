using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Tmda.Core
{
    public interface IDeviceFactory<TDevice>
    {
        Task<IReadOnlyList<DeviceInfo>> GetDevices(CancellationToken cancel = default);
        Task<TDevice> Create(DeviceInfo device, CancellationToken cancel = default);
    }

    public class DeviceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
    }
}