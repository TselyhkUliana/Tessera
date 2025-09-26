namespace Tessera.PolinomProvider.Model
{
  public class SectionDefinition
  {
    /// <summary>Материал</summary>
    public string Material { get; set; }
    /// <summary>Список похожих материалов</summary>
    public List<string> SuggestedMaterials { get; set; } = new();
    /// <summary>Сортамент</summary>
    public string Sortament { get; set; }
    /// <summary>Список похожих сортаментов</summary>
    public List<string> SuggestedSortament { get; set; } = new();
    /// <summary>Типоразмер</summary>
    public string TypeSize { get; set; }
    /// <summary>Экземпляр сортамента</summary>
    public string SortamentEx { get; set; }

    public TypeSizeData TypeSizeData { get; set; }
  }
}
