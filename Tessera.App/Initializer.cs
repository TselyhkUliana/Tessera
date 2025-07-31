using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tessera.App.Command;
using Tessera.App.ViewModel;

namespace Tessera.App
{
  internal class Initializer
  {
    public IEnumerable<ICommand> GetCommand(ObservableCollection<SectionDefinitionViewModel> sectionDefinitions, ISuggestionProvider suggestionProvider)
    {
      yield return new AddCommand(sectionDefinitions, suggestionProvider);
      yield return new RemoveCommand();
    }
  }
}
