using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.PolinomProvider.Interface;

namespace Tessera.App.Command
{
  internal class RemoveCommand : DelegateCommandBase
  {
    public RemoveCommand(IReferenceProvider referenceProvider)
    {
      Caption = "_Удалить";
      Hint = "Удалить текущую секцию (колонку)";
      Name = "RemoveCommand";
    }

    protected override void Execute(object parameter)
    {
      
    }

    protected override bool CanExecute(object parameter)
    {
      return base.CanExecute(parameter);
    }
  }
}
