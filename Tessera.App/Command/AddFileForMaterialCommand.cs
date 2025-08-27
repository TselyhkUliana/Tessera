using Microsoft.Practices.Prism.Commands;
using Tessera.App.ViewModel;

namespace Tessera.App.Command
{
  class AddFileForMaterialCommand : DelegateCommandBase
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
    }

    protected override bool CanExecute(object parameter)
    {
      return true;
    }
  }
}
