using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public interface IViewModelBase: IScreen
    {
        string Id { get; }
        string IconName { get; }
    }
}