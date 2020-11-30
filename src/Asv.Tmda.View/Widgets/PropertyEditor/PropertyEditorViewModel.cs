using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public class PropertyEditorViewModel:Conductor<IPropertyViewModel>.Collection.AllActive
    {
        private bool _isVisible = true;
        private string _description;
        private bool _isGroupsEnabled;

        public PropertyEditorViewModel()
        {
            
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
        }

       
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if (IsGroupsEnabled)
            {
                InternalEnableGroups();
            }
            else
            {
                InternalDisableGroups();
            }
        }

        public bool IsGroupsEnabled
        {
            get { return _isGroupsEnabled; }
            set
            {
                _isGroupsEnabled = value;
                if (value)
                {
                    InternalEnableGroups();
                }
                else
                {
                    InternalDisableGroups();
                }
                
                
            }
        }

        private void InternalDisableGroups()
        {
            var viewObj = GetView();
            if (viewObj == null)
            {
                // not created yet
                return;
            }

            var pe = (viewObj as PropertyEditorView);
            Debug.Assert(pe != null, "Unknown view");
            var cv = CollectionViewSource.GetDefaultView(Items);
            cv.GroupDescriptions.Clear();
        }

        private void InternalEnableGroups()
        {
            var viewObj = GetView();
            if (viewObj == null)
            {
                // not created yet
                return;
            }

            var pe = (viewObj as PropertyEditorView);
            Debug.Assert(pe != null, "Unknown view");
            var cv = CollectionViewSource.GetDefaultView(Items);
            if (cv.GroupDescriptions.Any()) return;
            cv.GroupDescriptions.Add(new PropertyGroupDescription(nameof(IPropertyViewModel.Group)));
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (value == _description) return;
                _description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (value == _isVisible) return;
                _isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }
    }

    public static class PropertyEditorHelper
    {
        
    }
}
