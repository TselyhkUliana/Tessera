using Ascon.Polynom.Api;

namespace Tessera.App.PolinomProvider
{
  public class FileAttachmentEventArgs : EventArgs
  {
    public FileAttachmentEventArgs(IElement element, byte[] fileBody, string fileName, string documentGroupName)
    {
      Element = element;
      FileName = fileName;
      FileBody = fileBody;
      DocumentGroupName = documentGroupName;
    }

    public IElement Element { get; }
    public byte[] FileBody { get; }
    public string FileName { get; }
    public string DocumentGroupName { get; }
  }
}
