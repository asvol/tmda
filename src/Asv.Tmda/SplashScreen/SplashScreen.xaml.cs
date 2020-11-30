using System.Reflection;
using System.Windows;
using Asv.Avialab.Core;

namespace Asv.Tmda
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
            Version.Text = Assembly.GetCallingAssembly().GetInformationalVersion();
        }
    }
}
