using Ascon.Polynom.Api;
using Tessera.App.PolinomHandlers;
using Tessera.App.PolinomHandlers.Utils.Constants;
using Tessera.App.ViewModel;

namespace Tessera.App.Command
{
  class AddFileForSortamentCommand : AddFileCommandBase
  {
    public AddFileForSortamentCommand() : base()
    {
      Caption = "Добавить файл для сортамента";
      Hint = "Добавить файл для сортамента";
      Name = "AddFileForSortamentCommand";
    }

    public event Action<IElement> SortamentCreated;

    protected override void Execute(object parameter)
    {
      var sectionDefinitions = parameter as SectionDefinitionViewModel;
      var polinomHandler = PolinomHandler.Instance;

      var (name, body) = GetSelectedFile();
      if (name == null || body == null)
        return;
      polinomHandler.AttachFileToDocument(name, body, sectionDefinitions.Material, CatalogConstants.CATALOG_SORTAMENT);
    }

    protected override bool CanExecute(object parameter)
    {
      return true;
    }
  }
}
