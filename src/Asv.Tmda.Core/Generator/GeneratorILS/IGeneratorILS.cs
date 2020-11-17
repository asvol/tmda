using System.Threading;
using System.Threading.Tasks;

namespace Asv.Tmda.Core
{
    public class ModulationILSConfig
    {

    }

    public interface IGeneratorILS : IGeneratorSimple
    {
        Task SetModulation(ModulationILSConfig cfg, CancellationToken cancel = default);  
    }
}