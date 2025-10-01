using Tessera.App.ViewModel;
using Tessera.PolinomProvider.Constants;
using Tessera.PolinomProvider.Interface;

namespace Tessera.App.Command
{
  class AddFileForMaterialCommand : AddFileCommandBase
  {
    private readonly IReferenceProvider _referenceProvider;

    public AddFileForMaterialCommand(IReferenceProvider referenceProvider) : base()
    {
      Caption = "Добавить файл для материала";
      Hint = "Добавляет файл для документа материала в ПОЛИНОМ:MDM";
      Name = "AddFileForMaterial";
      Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Tessera.App;component/Resources/Images/AttachMaterialFile.png"));
      _referenceProvider = referenceProvider;
    }

    protected override void Execute(object parameter)
    {
      //var sectionDefinitions = parameter as SectionDefinitionViewModel;

      //var (name, body) = GetSelectedFile();
      //if (name == null || body == null)
      //  return;
      //_referenceProvider.AttachFileToDocument(name, body, sectionDefinitions.Material, CatalogConstants.CATALOG_MATERIAL);
    }

    protected override bool CanExecute(object parameter)
    {
      return true;
    }
  }
}