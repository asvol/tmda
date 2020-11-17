using System.Threading;
using System.Threading.Tasks;

namespace Asv.Tmda.Core
{
    public interface IGeneratorSimple
    {
        Task RfOnOff(bool onOff, CancellationToken cancel = default);
        Task SetLevel(double leveldBm, CancellationToken cancel = default);
        Task SetFreq(double freqHz, CancellationToken cancel = default);
    }
}