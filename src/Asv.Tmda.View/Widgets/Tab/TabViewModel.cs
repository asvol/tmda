using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public class TabViewModel: Conductor<IViewModelBase>.Collection.OneActive
    {
        private IViewModelBase _selectedItem;
        public IViewModelBase SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (Equals(value, _selectedItem)) return;
                if (_selectedItem!=null) DeactivateItem(_selectedItem,false);
                _selectedItem = value;
                value?.Activate();
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }


    }
}