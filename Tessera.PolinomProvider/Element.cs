using System.Xml.Serialization;
using Tessera.PolinomProvider.Extensions;

namespace Tessera.PolinomProvider
{
  [Serializable]
  public class Element
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string GroupName { get; set; }
    public string Embedding { get; set; }
    public string Type { get; set; }
    [XmlIgnore]
    public float[] EmbeddingVector => Embedding.ParseEmbeddingVector();
  }

  [Serializable]
  [XmlRoot("Elements")]
  public class Elements
  {
    [XmlElement("Element")]
    public List<Element> Items { get; set; } = new();
  }

  public enum ElementType
  {
    Material,
    Sortament
  }
}
