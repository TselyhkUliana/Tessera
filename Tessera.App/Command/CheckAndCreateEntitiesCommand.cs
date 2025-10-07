using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using Tessera.App.ViewModel;
using Tessera.PolinomProvider.Interface;

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
      var sectionDefinitions = (parameter as ObservableCollection<SectionDefinitionViewModel>)
                               .Where(x => !string.IsNullOrEmpty(x.Material)
                               && !string.IsNullOrEmpty(x.Sortament)
                               && !string.IsNullOrEmpty(x.TypeSizeViewModel.TypeSize));

      foreach (var vm in sectionDefinitions)
      {
        _referenceProvider.TransactionManager.ApplyChanges(() =>
        { 
          var sectionDefinition = SectionMapper.ToDomain(vm);
          _referenceProvider.EnsureEntitiesExist(sectionDefinition);
          vm.SortamentEx = sectionDefinition.SortamentEx;
        });
      }
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