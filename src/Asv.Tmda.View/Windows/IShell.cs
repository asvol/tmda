using System;
using System.ComponentModel.Composition;
using System.Threading;
using Asv.Avialab.Core;
using Caliburn.Micro;

namespace Asv.Tmda.View
{
    public interface IShell:IScreen
    {
        void OpenSettings(string settingsTabId);
    }


    [Export(typeof(IShell))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShellViewModel : BaseWindowViewModel<Screen>, IShell, IDisposable
    {
        private CancellationTokenSource _cancel = new CancellationTokenSource();

        public ShellViewModel()
        {
            DisplayName = "TMDA";

            WindowHeader.OnIconClick.Subscribe(_ => Settings.OpenSettings(), _cancel.Token);
            WindowHeader.Tools.ActivateItem(new ChromelessButtonViewModel(Settings.OpenSettings) { DisplayName = "Config", IconName = "cog_outline" });

        }

        public void OpenSettings(string settingsTabId)
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}