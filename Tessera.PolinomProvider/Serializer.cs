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
    private static Elements _instance;
    private static XmlSerializer _xmlSerializer = new XmlSerializer(typeof(Elements));
    private static string _folderPath = @"D:\Development\Полином\Tessera";
    private static string _filePath = @$"{_folderPath}\ElementCatche.xml";
    public Elements Elements { get; set; }
    public static Elements Instance => _instance ??= LoadPropperties();

    public void Save()
    {
      using var stream = new FileStream(_filePath, FileMode.Create);
      _xmlSerializer.Serialize(stream, Elements);
    }

    private static Elements LoadPropperties()
    {
      var settings = new Elements();
      try
      {
        using var stream = new FileStream(_filePath, FileMode.Open);
        return _xmlSerializer.Deserialize(stream) as Elements;
      }
      catch
      {
        //что-то делать
      }
      return null;
    }
  }
}
