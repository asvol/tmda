using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tmda.Core;
using Asv.Tmda.SignalHound;

namespace Asv.Tmda
{
    class Program
    {
        private static CompositionContainer _container;

        static void Main(string[] args)
        {
            CreateIoCContainer();

            var a = _container.GetExportedValue<IAnalyzerIqFactory>();
            var devices = a.GetDevices(CancellationToken.None).Result;
            var arr = devices.Select(_ => a.Create(_, CancellationToken.None).Result).ToArray();
            foreach (var dev in arr)
            {
                dev.Open(CancellationToken.None);
                
            }
        }

        private static void CreateIoCContainer()
        {
            _container = new CompositionContainer(new AggregateCatalog(RegisterAssembly.Distinct().Select(_ => new AssemblyCatalog(_)).OfType<ComposablePartCatalog>()));
            var batch = new CompositionBatch();
            batch.AddExportedValue(_container);
            _container.Compose(batch);
        }

        private static IEnumerable<Assembly> RegisterAssembly
        {
            get
            {
                yield return typeof(Program).Assembly;
                yield return typeof(AnalyzerIqFactory).Assembly;
                yield return typeof(SaAnalyzerIq).Assembly;

            }
        }
    }
}
