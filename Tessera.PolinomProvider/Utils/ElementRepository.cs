using Ascon.Polynom.Api;
using Tessera.PolinomProvider.Constants;

namespace Tessera.PolinomProvider.Utils
{
  internal class ElementRepository
  {
    private readonly ISession _session;
    private PolinomApiHelper _polinomApiHelper;
    private IConcept _conceptSemanticRepresentation;

    public ElementRepository(ISession session, PolinomApiHelper polinomApiHelper)
    {
      _session = session;
      _polinomApiHelper = polinomApiHelper;
    }

    internal bool EnsureVectorConceptExists()
    {
      _conceptSemanticRepresentation = _polinomApiHelper.GetConceptByName(ConceptConstants.CONCEPT_SEMANTIC_REPRESENTATION_NAME);
      if (_conceptSemanticRepresentation is null)
      {
        _polinomApiHelper.ExecuteWithEditorClient(() =>
        {
          _conceptSemanticRepresentation = _session.Objects.CreateConcept(ConceptConstants.CONCEPT_SEMANTIC_REPRESENTATION_NAME, ConceptConstants.CONCEPT_SEMANTIC_REPRESENTATION_Code);
          var groupProp = _session.Objects.PropDefCatalog.CreatePropDefGroup(CatalogConstants.GROUP_PROP_SEMANTIC_REPRESENTATION);
          var prop = groupProp.CreateRtfPropertyDefinition(PropConstants.PROP_EMBEDDING_NAME, PropConstants.PROP_EMBEDDING_CODE);
          _conceptSemanticRepresentation.CreatePropertySource(prop);
          var conceptPropSource = _conceptSemanticRepresentation.ConceptPropertySources.FirstOrDefault(x => x.AbsoluteCode == PropConstants.PROP_EMBEDDING_ABSOLUTE_CODE);
          conceptPropSource.IsHidden = true;
          conceptPropSource.IsReadOnly = true;
        });
        return true;
      }
      return false;
    }

    internal void CreateElementEmbedding(string location, string embedding)
    {
      var element = _session.Objects.Locate(location) as IElement;
      element.RealizeContract(_conceptSemanticRepresentation);
      var prop = element.GetProperty(PropConstants.PROP_EMBEDDING_CODE);
      prop.AssignPropertyValue((prop.Definition as IRtfPropertyDefinition).CreateRtfPropertyValueData(embedding));
    }

    internal async Task<Elements> LoadElementsWithEmbeddingAsync()
    {
      var elements = await Serializer.Instance;
      if (elements is null)
      {
        var (materials, sortament) = await LoadElementsAsync();
        elements = await BuildAndSaveElementsAsync(materials, sortament);
      }

      return elements;
    }

    private static async Task<Elements> BuildAndSaveElementsAsync(List<IElement> materials, List<IElement> sortament)
    {
      return await Task.Run(() =>
      {
        var elements = BuildElements(materials, sortament);
        SaveElements(elements);
        return elements;
      });
    }

    private static IEnumerable<Element> MapToElements(IEnumerable<IElement> source, string type)
    {
      return source.Select(e =>
      {
        var prop = e.GetPropertyValue(PropConstants.PROP_EMBEDDING_CODE) as IRtfPropertyValue;
        var plain = prop?.ToPlainText() ?? string.Empty;
        return new Element
        {
          Id = e.Id,
          Name = e.Name,
          GroupName = e.OwnerGroup.Name,
          Embedding = plain,
          Type = type
        };
      });
    }

    private static Elements BuildElements(List<IElement> materials, List<IElement> sortament)
    {
      return new Elements
      {
        Items = MapToElements(materials, ElementType.Material.ToString())
                .Concat(MapToElements(sortament, ElementType.Sortament.ToString()))
                .ToList()
      };
    }

    private static void SaveElements(Elements elements)
    {
      var serializer = new Serializer { Elements = elements };
      serializer.Save();
    }

    internal async Task<IEnumerable<(string Name, string Location)>> LoadElementsForEmbeddingAsync()
    {
      var elements = await LoadElementsAsync();

      return elements.Materials
          .Select(x => (x.Name, x.GetBOSimpleLocation()))
          .Concat(elements.Sortament
              .Select(x => (x.Name, x.GetBOSimpleLocation())))
          .ToList();
    }

    internal async Task<(List<IElement> Materials, List<IElement> Sortament)> LoadElementsAsync()
    {
      var catalogSortament = _polinomApiHelper.GetCatalog(CatalogConstants.CATALOG_SORTAMENT);
      var catalogMaterial = _polinomApiHelper.GetCatalog(CatalogConstants.CATALOG_MATERIAL);
      var materialsTask = Task.Run(() => GetAllForGroupElements(catalogMaterial.Groups));
      var sortamentTask = Task.Run(() => GetAllForGroupElements(catalogSortament.Groups));
      await Task.WhenAll(materialsTask, sortamentTask);
      return (materialsTask.Result, sortamentTask.Result);
    }

    internal List<IElement> GetAllForGroupElements(IApiReadOnlyCollection<IGroup> groups)
    {
      var result = new List<IElement>();
      foreach (var group in groups)
      {
        result.AddRange(group.Elements);
        result.AddRange(GetAllForGroupElements(group.Groups));
      }
      return result;
    }

    //public List<IElement> GetAllElements(IApiReadOnlyCollection<IGroup> groups) => groups.SelectMany(g => g.Elements.Concat(GetAllElements(g.Groups))).ToList();

    //public List<string> Test()
    //{
    //  var list = new List<string>();
    //  var group = FindGroupByName(_session.Objects.PropDefCatalog.PropDefGroups, prop => prop.PropDefGroups, "Для обозначений");
    //  foreach (var item in group.PropDefGroups)
    //  {
    //    foreach (var item1 in item.PropertyDefinitions)
    //    {
    //      list.Add(item1.Name.ToString());
    //    }
    //  }

    //  //foreach (var item in list.Distinct())
    //  //{
    //  //  Debug.WriteLine(item);
    //  //}
    //  return list.Distinct().ToList();
    //}
  }
}