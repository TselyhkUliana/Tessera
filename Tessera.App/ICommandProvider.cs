using System.Collections.ObjectModel;
using System.Windows.Input;
using Tessera.App.ViewModel;

namespace Tessera.App
{
  interface ICommandProvider
  {
    IEnumerable<ICommand> GetCommands(ObservableCollection<SectionDefinitionViewModel> sectionDefinitions, ISuggestionProvider suggestionProvider);
  }
}
