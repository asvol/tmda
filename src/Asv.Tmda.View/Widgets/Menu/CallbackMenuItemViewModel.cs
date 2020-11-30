using System;

namespace Asv.Avialab.Core
{
    public class CallbackMenuItemViewModel : MenuItemViewModel
    {
        private readonly Action<CallbackMenuItemViewModel> _execute;

        public CallbackMenuItemViewModel(string displayName, Action<CallbackMenuItemViewModel> execute, bool isVisible, string iconName) : this(displayName,
            execute, isVisible)
        {
            IconName = iconName;
        }

        public CallbackMenuItemViewModel(string displayName, Action<CallbackMenuItemViewModel> execute, bool isVisible) : this(displayName, execute)
        {
            IsVisible = isVisible;
        }

        public CallbackMenuItemViewModel(string displayName, Action<CallbackMenuItemViewModel> execute) : this(execute)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            DisplayName = displayName;
        }

        public CallbackMenuItemViewModel(Action<CallbackMenuItemViewModel> execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = execute;
        }

        public override void Execute()
        {
            _execute(this);
        }


    }
}