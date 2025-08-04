using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tessera.App.ViewModel;

namespace Tessera.App.Command
{
  internal class AddCommand : DelegateCommandBase
  {
    //private readonly ObservableCollection<SectionDefinitionViewModel> _sectionDefinitions;
    private readonly ISuggestionProvider _suggestionProvider;

    public AddCommand(ObservableCollection<SectionDefinitionViewModel> sectionDefinitions, ISuggestionProvider suggestionProvider) : base()
    {
      Caption = "_Добавить";
      Hint = "Добавить новую секцию (колонку)";
      //_sectionDefinitions = sectionDefinitions;
      _suggestionProvider = suggestionProvider;
    }

    protected override void Execute(object parameter)
    {
      var test = ReferenceProvider.Instance;
      var sectionDefinitions = parameter as ObservableCollection<SectionDefinitionViewModel>;
      test.Find(sectionDefinitions);
    }

    protected override bool CanExecute(object parameter)
    {
      return true;
    }
  }
}
