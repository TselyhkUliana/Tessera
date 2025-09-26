using Tessera.App.ViewModel;
using Tessera.PolinomProvider;
using Tessera.PolinomProvider.Constants;

namespace Tessera.App.Command
{
  class AddFileForSortamentCommand : AddFileCommandBase
  {
    private readonly IReferenceProvider _referenceProvider;

    public AddFileForSortamentCommand(IReferenceProvider referenceProvider) : base()
    {
      Caption = "Добавить файл для сортамента";
      Hint = "Добавляет файл для документа сортамента в ПОЛИНОМ:MDM";
      Name = "AddFileForSortamentCommand";
      Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Tessera.App;component/Resources/Images/AttachSortamentFile.png"));
      _referenceProvider = referenceProvider;
    }

    protected override void Execute(object parameter)
    {
      var sectionDefinitions = parameter as SectionDefinitionViewModel;

      var (name, body) = GetSelectedFile();
      if (name == null || body == null)
        return;
      _referenceProvider.AttachFileToDocument(name, body, sectionDefinitions.Material, CatalogConstants.CATALOG_SORTAMENT);
    }

    protected override bool CanExecute(object parameter)
    {
      return true;
    }
  }
}
