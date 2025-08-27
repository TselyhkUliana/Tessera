using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using System.IO;

namespace Tessera.App.Command
{
  abstract class AddFileCommandBase : DelegateCommandBase
  {
    public AddFileCommandBase() : base() { }

    protected (string name, byte[] body) GetSelectedFile()
    {
      OpenFileDialog openFileDialog = new()
      {
        Title = "Выберите файл для прикрепления к документу",
        Filter = "All files (*.*)|*.*",
      };
      if (openFileDialog.ShowDialog() != true)
        return (null, null);
      var fileName = openFileDialog.FileName;
      var name = new FileInfo(fileName).Name;
      var body = File.ReadAllBytes(fileName);

      return (name, body);
    }
  }
}
