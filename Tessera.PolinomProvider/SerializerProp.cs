using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Tessera.PolinomProvider.Extensions;

namespace Tessera.PolinomProvider
{
  internal class SerializerProp
  {
    private static PropertyList _instance;
    private static XmlSerializer _xmlSerializer = new XmlSerializer(typeof(PropertyList));
#if DEBUG
    private static string _folderPath = @"D:\Development\Полином\Tessera\Harold";
    private static string _filePath = @$"{_folderPath}\Properties.xml";
#else
    private static string _filePath =  @$"{AppContext.BaseDirectory}Properties.xml";
#endif
    public PropertyList MyProperty { get; set; }
    public static PropertyList Instance => _instance ??= LoadPropperties();

    [Obsolete]
    public void Save()
    {
      using var stream = new FileStream(_filePath, FileMode.Create);
      _xmlSerializer.Serialize(stream, MyProperty);
    }

    private static PropertyList LoadPropperties()
    {
      var settings = new PropertyList();
      try
      {
        using var stream = new FileStream(_filePath, FileMode.Open);
        return _xmlSerializer.Deserialize(stream) as PropertyList;
      }
      catch
      {
        //что-то делать
      }
      return null;
    }
  }

  [Serializable]
  public class PropertyItem
  {
    public string Name { get; set; }
    public string Category { get; set; }
    public int Priority { get; set; }
    public string Embedding { get; set; }

    [XmlIgnore]
    public float[] EmbeddingVector => Embedding.ParseEmbeddingVector();
  }

  [Serializable]
  [XmlRoot("Properties")]
  public class PropertyList
  {
    [XmlElement("Property")]
    public List<PropertyItem> Items { get; set; } = new();
  }
}
