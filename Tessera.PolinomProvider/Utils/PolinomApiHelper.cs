using Ascon.Polynom.Api;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tessera.PolinomProvider;
using Tessera.PolinomProvider.Constants;

namespace Tessera.PolinomProvider.Utils
{
  internal class PolinomApiHelper
  {
    private readonly ISession _session;
    private readonly IReference? _referenceMaterialAndSortament;
    private readonly IApiReadOnlyCollection<IPropertyDefinition> _propDefinitions;
    private readonly TransactionManager _transactionManager;
    private readonly PropertyList _property;
    private IConcept _conceptSemanticRepresentation;

    public PolinomApiHelper(ISession session, TransactionManager transactionManager)
    {
      _session = session;
      _transactionManager = transactionManager;
      _referenceMaterialAndSortament = _session.Objects.AllReferences.FirstOrDefault(x => x.Name == CatalogConstants.REFERENCE_NAME);
      _propDefinitions = FindGroupByName(_session.Objects.PropDefCatalog.PropDefGroups, prop => prop.PropDefGroups, CatalogConstants.GROUP_PROP_DEFINABLE_DIMENSION).PropertyDefinitions;
      _property = SerializerProp.Instance;
    }

    public IApiReadOnlyCollection<IPropertyDefinition> PropertyDefinitions => _propDefinitions;

    public void SetClientType(ClientType clientType) => _session.ClientType = clientType;

    public IConcept GetConceptByAbsoluteCode(string absoluteCode) => _session.Objects.Get<IConcept>(absoluteCode);

    public IConcept GetConceptByName(string conceptName) => _session.Objects.AllConcepts.FirstOrDefault(c => c.Name == conceptName);

    public ICatalog GetCatalog(string catalogName) => _referenceMaterialAndSortament.Catalogs.FirstOrDefault(x => x.Name == catalogName);

    public IDocumentCatalog GetDocumentCatalog() => _referenceMaterialAndSortament.DocumentCatalog;

    public IApiReadOnlyCollection<IFormulaGroup> GetFormulaGroups() => _session.Objects.FormulaCatalog.FormulaGroups;

    public Dictionary<string, PropertyType> GetProperties()
    {
      return _propDefinitions.OrderBy(x => x.Name).ToDictionary
                              (p => p.Name,
                               p => Enum.Parse<PropertyType>(p.Type.ToString()));
    }

    public void ExecuteWithEditorClient(Action action)
    {
      SetClientType(ClientType.Editor);
      action?.Invoke();
      SetClientType(ClientType.Client);
    }

    public bool EnsureVectorConceptExists()
    {
      _conceptSemanticRepresentation = GetConceptByName(ConceptConstants.CONCEPT_SEMANTIC_REPRESENTATION_NAME);
      if (_conceptSemanticRepresentation is null)
      {
        ExecuteWithEditorClient(() =>
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

    public void CreateElementEmbedding(string location, string embedding)
    {
      var element = _session.Objects.Locate(location) as IElement;
      element.RealizeContract(_conceptSemanticRepresentation);
      var prop = element.GetProperty(PropConstants.PROP_EMBEDDING_CODE);
      prop.AssignPropertyValue((prop.Definition as IRtfPropertyDefinition).CreateRtfPropertyValueData(embedding));
    }

    public async Task<Elements> LoadElementsWithEmbeddingAsync()
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

    public async Task<IEnumerable<(string Name, string Location)>> LoadElementsForEmbeddingAsync()
    {
      var elements = await LoadElementsAsync();

      return elements.Materials
          .Select(x => (x.Name, x.GetBOSimpleLocation()))
          .Concat(elements.Sortament
              .Select(x => (x.Name, x.GetBOSimpleLocation())))
          .ToList();
    }

    public async Task<(List<IElement> Materials, List<IElement> Sortament)> LoadElementsAsync()
    {
      var catalogSortament = GetCatalog(CatalogConstants.CATALOG_SORTAMENT);
      var catalogMaterial = GetCatalog(CatalogConstants.CATALOG_MATERIAL);
      var materialsTask = Task.Run(() => GetAllForGroupElements(catalogMaterial.Groups));
      var sortamentTask = Task.Run(() => GetAllForGroupElements(catalogSortament.Groups));
      await Task.WhenAll(materialsTask, sortamentTask);
      return (materialsTask.Result, sortamentTask.Result);
    }

    public List<IElement> GetAllForGroupElements(IApiReadOnlyCollection<IGroup> groups)
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

    public Task<List<string>> GetPropertiesForTypeSizeInternal(string similarSortament)
    {
      return Task.Run(() =>
      {
        var sortament = SearchElement(similarSortament, CatalogConstants.CATALOG_SORTAMENT);
        var typeSizeLink = sortament.Links.FirstOrDefault(l => l.Name == LinkConstants.LINK_TYPE_SIZE_SORTAMENT_NAME);
        var typeSizeElement = (IElement)typeSizeLink.LinkedItems.First();
        var dimensionContract = typeSizeElement.AllContracts.FirstOrDefault(c => c.Name.Contains("Размерность"));
        return dimensionContract.AllPropertySources.Select(x => x.Definition.Name).ToList();
      });
    }

    public IDocument AddOrCreateDocument(IElement element, string fullName, string documentGroupName)
    {
      var fullStandard = EntityNameHelper.GetFullStandard(fullName);
      if (string.IsNullOrEmpty(fullStandard))
        return null;

      var documentGroup = GetDocumentCatalog().DocumentGroups.FirstOrDefault(x => x.Name == documentGroupName);
      var document = SearchDocument(fullStandard) ?? CreateDocument(fullName, fullStandard, documentGroup);

      if (!IsInGroupPath(document.OwnerGroup, documentGroupName))
        AddDocument(fullStandard, documentGroup, document);

      document.Applicability = Applicability.Allowed;
      element.LinkDocument(document);
      return document;
    }

    public void AddDocument(string fullStandard, IDocumentGroup documentGroup, IDocument document)
    {
      var standard = EntityNameHelper.GetStandard(fullStandard);
      var groupDocument = FindGroupByName(documentGroup.DocumentGroups, g => g.DocumentGroups, standard) ?? documentGroup.CreateDocumentGroup(standard);
      groupDocument.AddDocument(document);
    }

    public IDocument CreateDocument(string fullName, string fullStandard, IDocumentGroup documentGroup)
    {
      var baseName = EntityNameHelper.FormatNameParts(fullName);
      var standard = EntityNameHelper.GetStandard(fullStandard);
      var groupDocument = FindGroupByName(documentGroup.DocumentGroups, g => g.DocumentGroups, standard) ?? documentGroup.CreateDocumentGroup(standard);
      if (StandardConstants.SpecialDocumentStandards.Contains(standard))
        return groupDocument.CreateDocument(fullStandard, null, fullStandard);
      if (StandardConstants.DocumentStandards.Contains(standard))
        return FindGroupByName(documentGroup.DocumentGroups, g => g.DocumentGroups, standard).CreateDocument(fullStandard, null, fullStandard);
      if (standard is StandardConstants.STANDARD)
        return groupDocument.CreateDocument(fullStandard, null, fullStandard);
      if (StandardConstants.DocumentTuStandards.FirstOrDefault(x => fullStandard.StartsWith(x.Key)) is var kvp && !string.IsNullOrEmpty(kvp.Value))
        return FindGroupByName(documentGroup.DocumentGroups, g => g.DocumentGroups, kvp.Value).CreateDocument(fullStandard, null, fullStandard);

      var (Name, Description) = EntityNameHelper.GetNameAndDescriptionForDocument(fullStandard, baseName);
      return groupDocument.CreateDocument(Name, Description, fullStandard);
    }

    public void AttachFile(IElement element, string fileName, byte[] fileBody, string documentGroupName)
    {
      var document = element.Documents.FirstOrDefault() ?? AddOrCreateDocument(element, element.Name, documentGroupName);
      document.CreateFile(fileName, fileBody);
    }

    public IElement SearchElement(string element, string catalogName)
    {
      return SearchEntity(KnownConceptKind.Element, KnownPropertyDefinitionKind.Name, element, condition => GetCatalog(catalogName).Intersect(condition).GetEnumerable<IElement>());
    }

    public IDocument SearchDocument(string standard)
    {
      return SearchEntity(KnownConceptKind.Document, KnownPropertyDefinitionKind.Designation, standard, condition => GetDocumentCatalog().Intersect(condition).GetEnumerable<IDocument>());
    }

    public void CreateLink(ILinkable left, ILinkable right, string aboluteCodeLink)
    {
      var group = _session.Objects.LinkDefCatalog.LinkDefGroups.FirstOrDefault(g => g.Name == CatalogConstants.REFERENCE_NAME);
      var link = group.LinkDefinitions.FirstOrDefault(l => l.AbsoluteCode == aboluteCodeLink);
      link.Destination.CreateLink(left, right);
    }

    public T FindGroupByName<T>(IApiReadOnlyCollection<T> groups, Func<T, IApiReadOnlyCollection<T>> childrenSelector, string groupName) where T : class, IBaseGroup
    {
      foreach (var group in groups)
      {
        if (group.Name == groupName)
          return group;
        var found = FindGroupByName(childrenSelector(group), childrenSelector, groupName);
        if (found is not null)
          return found;
      }
      return null;
    }

    public bool IsInGroupPath(IDocumentGroup group, string findGroupName)
    {
      return group.Name == findGroupName || group.ParentGroup is not null && IsInGroupPath(group.ParentGroup, findGroupName);
    }

    public IFormula CreateOrReceiveFormula(string sortamentGroup, string formulaName, string groupName, IConcept conceptPropertiesByStandard)
    {
      var formulaGroups = GetFormulaGroups();
      var groupFormula = FindGroupByName(formulaGroups, g => g.FormulaGroups, sortamentGroup) ??
                         FindGroupByName(formulaGroups, g => g.FormulaGroups, groupName).CreateFormulaGroup(sortamentGroup);
      var formula = groupFormula.Formulas.FirstOrDefault(x => x.Name == formulaName) ?? CreateFormula(groupFormula, formulaName, conceptPropertiesByStandard);
      return formula;
    }

    private T SearchEntity<T>(KnownConceptKind conceptKind, KnownPropertyDefinitionKind propertyDefinitionKind, string value, Func<ICondition, IEnumerable<T>> sourceProvider) where T : class
    {
      var concept = _session.Objects.GetKnownConcept(conceptKind);
      var propDef = _session.Objects.GetKnownPropertyDefinition(propertyDefinitionKind);
      var condition = _session.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(value));
      return sourceProvider(condition).FirstOrDefault();
    }

    private IFormula CreateFormula(IFormulaGroup groupFormula, string name, IConcept conceptPropertiesByStandard)
    {
      var formula = groupFormula.CreateFormula(name, FormulaDefaults.BuildFormulaBodyDesignation());
      foreach (var (Name, Expression) in FormulaDefaults.Parameters)
        formula.CreateParameter(Name, Expression);
      foreach (var prop in conceptPropertiesByStandard.PropertySources)
      {
        var (Name, Expression) = FormulaDefaults.BuildParameterByDefinition(prop.GetPropertyDefinition(), prop.AbsoluteCode);
        formula.CreateParameter(Name, Expression);
      }
      return formula;
    }

    public void LinksTest(IElement element)
    {
      foreach (var link in element.Links)
      {
        Debug.WriteLine(link.Name);
        Debug.WriteLine("\tСвязанные объекты:");
        foreach (var linkedElement in link.LinkedItems)
        {
          Debug.WriteLine("\t\t" + ((IElement)linkedElement).Name + $" -  -  - {((IElement)linkedElement).ObjectId}");
        }
      }
    }
  }
}