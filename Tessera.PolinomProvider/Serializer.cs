using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Tessera.PolinomProvider
{
  internal class Serializer
  {
    private static Task<Elements> _instance;
    private static XmlSerializer _xmlSerializer = new XmlSerializer(typeof(Elements));
#if DEBUG
    private static string _folderPath = @"D:\Development\Полином";
    private static string _filePath = @$"{_folderPath}\ElementCatche.xml";
#else
    private static string _filePath =  @$"{AppContext.BaseDirectory}ElementCatche.xml";
#endif
    public Elements Elements { get; set; }
    public static Task<Elements> Instance => _instance ??= LoadElements();

    public void Save()
    {
      using var stream = new FileStream(_filePath, FileMode.Create);
      _xmlSerializer.Serialize(stream, Elements);
    }

    private static async Task<Elements> LoadElements()
    {
      var settings = new Elements();
      try
      {
        await using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        return await Task.Run(() => _xmlSerializer.Deserialize(stream) as Elements);
      }
      catch
      {
        //что-то делать
      }
      return null;
    }
  }
}
