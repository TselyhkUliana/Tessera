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

    public PolinomApiHelper(ISession session)
    {
      _session = session;
      _referenceMaterialAndSortament = _session.Objects.AllReferences.FirstOrDefault(x => x.Name == CatalogConstants.REFERENCE_NAME);
      _propDefinitions = FindGroupByName(_session.Objects.PropDefCatalog.PropDefGroups, prop => prop.PropDefGroups, CatalogConstants.GROUP_PROP_DEFINABLE_DIMENSION).PropertyDefinitions;
    }

    internal IApiReadOnlyCollection<IPropertyDefinition> PropertyDefinitions => _propDefinitions;

    internal void SetClientType(ClientType clientType) => _session.ClientType = clientType;

    internal IConcept GetConceptByAbsoluteCode(string absoluteCode) => _session.Objects.Get<IConcept>(absoluteCode);

    internal IConcept GetConceptByName(string conceptName) => _session.Objects.AllConcepts.FirstOrDefault(c => c.Name == conceptName);

    internal ICatalog GetCatalog(string catalogName) => _referenceMaterialAndSortament.Catalogs.FirstOrDefault(x => x.Name == catalogName);

    internal IDocumentCatalog GetDocumentCatalog() => _referenceMaterialAndSortament.DocumentCatalog;

    internal IApiReadOnlyCollection<IFormulaGroup> GetFormulaGroups() => _session.Objects.FormulaCatalog.FormulaGroups;

    internal Dictionary<string, PropertyType> GetProperties()
    {
      return _propDefinitions.OrderBy(x => x.Name).ToDictionary
                              (p => p.Name,
                               p => Enum.Parse<PropertyType>(p.Type.ToString()));
    }

    internal void ExecuteWithEditorClient(Action action)
    {
      SetClientType(ClientType.Editor);
      action?.Invoke();
      SetClientType(ClientType.Client);
    }

    internal Task<List<string>> GetPropertiesForTypeSizeInternal(string similarSortament)
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

    internal IDocument AddOrCreateDocument(IElement element, string fullName, string documentGroupName)
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

    internal void AddDocument(string fullStandard, IDocumentGroup documentGroup, IDocument document)
    {
      var standard = EntityNameHelper.GetStandard(fullStandard);
      var groupDocument = FindGroupByName(documentGroup.DocumentGroups, g => g.DocumentGroups, standard) ?? documentGroup.CreateDocumentGroup(standard);
      groupDocument.AddDocument(document);
    }

    internal IDocument CreateDocument(string fullName, string fullStandard, IDocumentGroup documentGroup)
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

      var (name, description) = EntityNameHelper.GetNameAndDescriptionForDocument(fullStandard, baseName);
      return groupDocument.CreateDocument(name, description, fullStandard);
    }

    internal void AttachFile(IElement element, string fileName, byte[] fileBody, string documentGroupName)
    {
      var document = element.Documents.FirstOrDefault() ?? AddOrCreateDocument(element, element.Name, documentGroupName);
      document.CreateFile(fileName, fileBody);
    }

    internal IElement SearchElement(string element, string catalogName)
    {
      return SearchEntity(KnownConceptKind.Element, KnownPropertyDefinitionKind.Name, element, condition => GetCatalog(catalogName).Intersect(condition).GetEnumerable<IElement>());
    }

    internal IDocument SearchDocument(string standard)
    {
      return SearchEntity(KnownConceptKind.Document, KnownPropertyDefinitionKind.Designation, standard, condition => GetDocumentCatalog().Intersect(condition).GetEnumerable<IDocument>());
    }

    internal void CreateLink(ILinkable left, ILinkable right, string aboluteCodeLink)
    {
      var group = _session.Objects.LinkDefCatalog.LinkDefGroups.FirstOrDefault(g => g.Name == CatalogConstants.REFERENCE_NAME);
      var link = group.LinkDefinitions.FirstOrDefault(l => l.AbsoluteCode == aboluteCodeLink);
      link.Destination.CreateLink(left, right);
    }

    internal T FindGroupByName<T>(IApiReadOnlyCollection<T> groups, Func<T, IApiReadOnlyCollection<T>> childrenSelector, string groupName) where T : class, IBaseGroup
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

    internal bool IsInGroupPath(IDocumentGroup group, string findGroupName)
    {
      return group.Name == findGroupName || group.ParentGroup is not null && IsInGroupPath(group.ParentGroup, findGroupName);
    }

    internal IFormula CreateOrReceiveFormula(string sortamentGroup, string formulaName, IConcept conceptPropertiesByStandard)
    {
      var formulaGroups = GetFormulaGroups();
      var groupFormula = FindGroupByName(formulaGroups, g => g.FormulaGroups, sortamentGroup) ??
                         FindGroupByName(formulaGroups, g => g.FormulaGroups, CatalogConstants.GROUP_FORMULA_DESIGNATION_SORTAMENT_EX).CreateFormulaGroup(sortamentGroup);
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

    private IFormula CreateFormula(IFormulaGroup groupFormula, string formulaName, IConcept conceptPropertiesByStandard)
    {
      var formula = groupFormula.CreateFormula(formulaName, FormulaBuilder.BuildFormulaBodyDesignation(null));
      foreach (var (name, expression) in FormulaBuilder.ParametersDefault)
        formula.CreateParameter(name, expression);
      foreach (var prop in conceptPropertiesByStandard.PropertySources)
      {
        var (baseName, name, expression) = FormulaBuilder.BuildParameterByDefinition(prop.GetPropertyDefinition(), prop.AbsoluteCode);
        formula.CreateParameter(name, expression);
      }
      return formula;
    }

    internal void LinksTest(IElement element)
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