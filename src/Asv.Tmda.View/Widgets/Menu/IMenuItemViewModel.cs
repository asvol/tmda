using System.ComponentModel;
using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public interface IMenuItemViewModel: IChild, INotifyPropertyChangedEx, IHaveDisplayName
    {
        string IconName { get; }
        int Order { get; }
        object Tag { get; set; }
        bool IsCheckable { get; }
        bool IsChecked { get; set; }
        bool IsVisible { get; }
        bool StaysOpenOnClick { get; }
        IObservableCollection<IMenuItemViewModel> Items { get; }
        void Execute();
    }
}