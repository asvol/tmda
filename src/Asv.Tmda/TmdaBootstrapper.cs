using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Windows;
using Asv.Tmda.Core;
using Asv.Tmda.View;
using Caliburn.Micro;
using NLog;
using LogManager = NLog.LogManager;


namespace Asv.Tmda
{
    public class TmdaBootstrapper : BootstrapperBase, IDisposable
    {
        private CompositionContainer _container;
        
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IEnumerable<IModule> _modules;

        public TmdaBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            var wnd = new SplashScreen();
            wnd.Show();

            _modules = _container.GetExportedValues<IModule>().ToArray();
            foreach (var module in _modules)
            {
                try
                {
                    module.Init();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error to init module {module}");
                }

            }

            _logger.Trace($"Startup with args: {string.Join(" ", e.Args)}");
            
            DisplayRootViewFor<IShell>();

            wnd.Close();
        }

        protected override void Configure()
        {
            //An aggregate catalog that combines multiple catalogs  
            var catalog = new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>());

            //Create the CompositionContainer with the parts in the catalog  
            _container = new CompositionContainer(catalog);

            var batch = new CompositionBatch();
            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(_container);


#if DEBUG
            batch.AddExportedValue<IConfiguration>(new JsonOneFileConfiguration("config.json", true, null));
#else
            batch.AddExportedValue<IConfiguration>(new JsonOneFileConfiguration(Path.Combine(paths.UserDataFolder, "config.json"), true, null));
#endif

            _container.Compose(batch);

            var defaultLocator = ViewLocator.LocateTypeForModelType;
            ViewLocator.LocateTypeForModelType = (modelType, displayLocation, context) =>
            {
                var viewType = defaultLocator(modelType, displayLocation, context);
                while (viewType == null && modelType != typeof(object))
                {
                    modelType = modelType.BaseType;
                    viewType = defaultLocator(modelType, displayLocation, context);
                }
                return viewType;
            };

        }

        protected override object GetInstance(Type serviceType, string key)
        {
            var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;

            var exports = _container.GetExportedValues<object>(contract);

            if (exports.Any())
                return exports.First();

            throw new Exception(string.Format("Could not locate instance", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            yield return typeof(TmdaBootstrapper).Assembly;
            yield return typeof(IShell).Assembly;

        }

        public void Dispose()
        {
            _container?.Dispose();
        }
    }
}