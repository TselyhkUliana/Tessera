using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.App.Command
{
  internal class RemoveCommand : DelegateCommandBase
  {
    public RemoveCommand()
    {
      Caption = "Удалить";
      Hint = "Удалить текущую секцию (колонку)";
    }

    protected override void Execute(object parameter)
    {
      
    }
  }
}
