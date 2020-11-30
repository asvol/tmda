using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Asv.Avialab.Core;
using NLog;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace Asv.Tmda
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private TmdaBootstrapper _boot;

        public App()
        {
            Current.DispatcherUnhandledException += (s, a) =>
            {
                _logger.Fatal(a.Exception, "App unhandled exception:{0}", a.Exception.Message);
                a.Handled =
                    MessageBox.Show(a.Exception.Message, a.Exception.ToString(), MessageBoxButton.YesNo,
                        MessageBoxImage.Error) == MessageBoxResult.Yes;
            };


            Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

            _logger.Info(typeof(App).Assembly.PrintWelcome());
            _boot = new TmdaBootstrapper();
        }
    }
}
