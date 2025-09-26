using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using Tessera.App.ViewModel;
using Tessera.PolinomProvider;

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
      foreach (var vm in sectionDefinitions.SkipLast(1))
        _referenceProvider.EnsureEntitiesExist(SectionMapper.ToDomain(vm));
    }

    protected override bool CanExecute(object parameter)
    {
      //if (parameter is ObservableCollection<SectionDefinitionViewModel> sectionDefinition && sectionDefinition.Count > 0)
      //{
      //  var first = sectionDefinition[0];
      //  return first.Material is not null &&
      //         first.Sortament is not null &&
      //         first.TypeSizeViewModel?.TypeSize is not null;
      //}
      //return false;
      return true;
    }
  }
}