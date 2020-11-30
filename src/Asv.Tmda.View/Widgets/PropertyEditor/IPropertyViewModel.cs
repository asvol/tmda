using System;
using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public enum State
    {
        None,
        Normal,
        Warning,
        Error,
        Progress,
    }

    public interface IPropertyViewModel : IScreen, ISupportValidation, IDisposable
    {
        State State { get; }
        string Group { get; }
        string StateMessage { get; }
        string IconName { get; }
        string Description { get; }
        bool IsEnabled { get; set; }
        bool IsVisible { get; set; }
    }

    public interface IEditable
    {
        bool IsEdit { get; set; }
    }


}
