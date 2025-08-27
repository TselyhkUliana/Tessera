using Microsoft.Practices.Prism.Commands;
using Tessera.App.ViewModel;

namespace Tessera.App.Command
{
  class AddFileForSortamentCommand : DelegateCommandBase
  {
    public AddFileForSortamentCommand() : base()
    {
      Caption = "Добавить файл для сортамента";
      Hint = "Добавить файл для сортамента";
      Name = "AddFileForSortamentCommand";
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
