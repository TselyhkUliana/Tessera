using Ascon.Polynom.Api;

namespace Tessera.App.PolinomHandlers
{
  public class FileAttachmentEventArgs : EventArgs
  {
    public FileAttachmentEventArgs(IElement element, string fileName, byte[] fileBody)
    {
      Element = element;
      FileName = fileName;
      FileBody = fileBody;
    }

    public IElement Element { get; }
    public string FileName { get; }
    public byte[] FileBody { get; }
  }
}
