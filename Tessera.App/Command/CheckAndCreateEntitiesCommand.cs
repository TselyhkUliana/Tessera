using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using Tessera.App.Polinom;
using Tessera.App.ViewModel;

namespace Tessera.App.Command
{
  internal class CheckAndCreateEntitiesCommand : DelegateCommandBase
  {
    private readonly ISuggestionProvider _suggestionProvider;
    private readonly IReferenceProvider _referenceProvider;

    public CheckAndCreateEntitiesCommand(ISuggestionProvider suggestionProvider, IReferenceProvider referenceProvider) : base()
    {
      Caption = "Создать недостающие элементы";
      Hint = "Автоматическое создание отсутствующих материалов, сортаментов и типоразмеров в ПОЛИНОМ:MDM";
      Name = "CheckAndCreateEntitiesCommand";
      _suggestionProvider = suggestionProvider;
      _referenceProvider = referenceProvider;
    }

    protected override void Execute(object parameter)
    {
      var sectionDefinitions = parameter as ObservableCollection<SectionDefinitionViewModel>;
      _referenceProvider.EnsureEntitiesExist(sectionDefinitions.First());
    }

    protected override bool CanExecute(object parameter)
    {
      return true;
    }
  }
}