using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Tmda.Core
{
    [Export(typeof(IDeviceFactory<IAnalyzerIq>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AnalyzerIqFactory : DeviceFactoryBase<IAnalyzerIq>
    {
        [ImportingConstructor]
        public AnalyzerIqFactory([ImportMany] IEnumerable<IDeviceProvider<IAnalyzerIq>> providers) : base(providers)
        {

        }

    }
}