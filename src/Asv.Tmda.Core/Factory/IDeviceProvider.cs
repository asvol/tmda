using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Tmda.Core
{
    public class ProviderDeviceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public interface IDeviceProvider<TDevice>
    {
        string TypeName { get; }
        Task<IReadOnlyList<ProviderDeviceInfo>> GetDevices(CancellationToken cancel = default);
        Task<TDevice> Create(string id, CancellationToken cancel = default);
    }
}