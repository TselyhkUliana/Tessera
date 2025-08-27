using Tessera.App.PolinomHandlers;
using Tessera.App.PolinomHandlers.Utils.Constants;
using Tessera.App.ViewModel;

namespace Tessera.App.Command
{
  class AddFileForMaterialCommand : AddFileCommandBase
  {
    public AddFileForMaterialCommand() : base()
    {
      Caption = "Добавить файл для материала";
      Hint = "Добавить файл для материала в ПОЛИНОМ:MDM";
      Name = "AddFileForMaterial";
    }

    protected override void Execute(object parameter)
    {
      var sectionDefinitions = parameter as SectionDefinitionViewModel;
      var polinomHandler = PolinomHandler.Instance;

      var (name, body) = GetSelectedFile();
      if (name == null || body == null)
        return;
      polinomHandler.AttachFileToDocument(name, body, sectionDefinitions.Material, CatalogConstants.CATALOG_MATERIAL);
    }

    protected override bool CanExecute(object parameter)
    {
      return true;
    }
  }
}