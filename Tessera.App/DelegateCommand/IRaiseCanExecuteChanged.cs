using System.Windows.Input;

namespace Microsoft.Practices.Prism.Commands
{
    public interface  IRaiseCanExecuteChangedCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
