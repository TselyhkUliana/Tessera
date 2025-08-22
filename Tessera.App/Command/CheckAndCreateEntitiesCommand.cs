using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tessera.App.PolinomHandlers;
using Tessera.App.ViewModel;

namespace Tessera.App.Command
{
  internal class CheckAndCreateEntitiesCommand : DelegateCommandBase
  {
    private readonly ISuggestionProvider _suggestionProvider;

    public CheckAndCreateEntitiesCommand(ObservableCollection<SectionDefinitionViewModel> sectionDefinitions, ISuggestionProvider suggestionProvider) : base()
    {
      Caption = "Создать недостающие элементы";
      Hint = "Автоматическое создание отсутствующих материалов, сортаментов и типоразмеров в ПОЛИНОМ:MDM";
      _suggestionProvider = suggestionProvider;
    }

    protected override void Execute(object parameter)
    {
      var sectionDefinitions = parameter as ObservableCollection<SectionDefinitionViewModel>;
      PolinomHandler.Instance.EnsureEntitiesExist(sectionDefinitions);
    }

    protected override bool CanExecute(object parameter)
    {
      return true;
    }
  }
}
