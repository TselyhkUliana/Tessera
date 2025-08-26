using Ascon.Polynom.Api;
using System.Diagnostics;
using Tessera.App.PolinomHandlers.Utils.Constants;

namespace Tessera.App.PolinomHandlers.Utils
{
  internal class PolinomApiHelper
  {
    private readonly ISession _session;
    private readonly TransactionManager _transactionManager;
    private readonly IReference? _referenceMaterialAndSortament;

    public PolinomApiHelper(ISession session, TransactionManager transactionManager)
    {
      _session = session;
      _transactionManager = transactionManager;
      _referenceMaterialAndSortament = _session.Objects.AllReferences.FirstOrDefault(x => x.Name == CatalogConstants.REFENCE_NAME);
    }

    public IConcept GetConceptByAbsoluteCode(string absoluteCode) => _session.Objects.Get<IConcept>(absoluteCode);

    public IConcept GetConceptByName(string conceptName) => _session.Objects.AllConcepts.FirstOrDefault(c => c.Name == conceptName);

    public ICatalog GetCatalog(string catalogName) => _referenceMaterialAndSortament.Catalogs.FirstOrDefault(x => x.Name == catalogName);

    public IDocumentCatalog GetDocumentCatalog() => _referenceMaterialAndSortament.DocumentCatalog;

    public IApiReadOnlyCollection<IFormulaGroup> GetFormulaGroups() => _session.Objects.FormulaCatalog.FormulaGroups;

    public void AddDocument(IElement element, string fullName, string documentGroupName)
    {
      var fullStandard = EntityNameHelper.GetFullStandard(fullName);
      var documentGroup = GetDocumentCatalog().DocumentGroups.FirstOrDefault(x => x.Name == documentGroupName);
      var document = SearchDocument(fullStandard) ?? CreateDocument(fullStandard, documentGroup);
      element.LinkDocument(document);
    }

    public IDocument CreateDocument(string fullStandard, IDocumentGroup documentGroup)
    {
      var standard = EntityNameHelper.GetStandard(fullStandard);
      var groupDocument = FindGroupByName(documentGroup.DocumentGroups, g => g.DocumentGroups, standard) ?? documentGroup.CreateDocumentGroup(standard);
      var document = groupDocument.CreateDocument(fullStandard);
      document.Designation = fullStandard;
      document.Applicability = Applicability.Allowed;
      return document;
    }

    public IElement SearchElement(string similarElement, string catalogName)
    {
      return SearchEntity(KnownConceptKind.Element, KnownPropertyDefinitionKind.Name, similarElement, condition => GetCatalog(catalogName).Intersect(condition).GetEnumerable<IElement>());
    }

    public IDocument SearchDocument(string standard)
    {
      return SearchEntity(KnownConceptKind.Document, KnownPropertyDefinitionKind.Designation, standard, condition => GetDocumentCatalog().Intersect(condition).GetEnumerable<IDocument>());
    }

    public void CreateLink(ILinkable left, ILinkable right, string aboluteCodeLink)
    {
      var group = _session.Objects.LinkDefCatalog.LinkDefGroups.FirstOrDefault(g => g.Name == CatalogConstants.REFENCE_NAME);
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

    public IFormula CreateOrReceiveFormula(string sortamentGroup, string formulaName, string groupName)
    {
      var formulaGroups = GetFormulaGroups();
      var groupFormula = FindGroupByName(formulaGroups, g => g.FormulaGroups, sortamentGroup) ??
                         FindGroupByName(formulaGroups, g => g.FormulaGroups, groupName).CreateFormulaGroup(sortamentGroup);
      var formula = groupFormula.Formulas.FirstOrDefault(x => x.Name == formulaName) ?? CreateFormula(groupFormula, formulaName);
      return formula;
    }

    private T SearchEntity<T>(KnownConceptKind conceptKind, KnownPropertyDefinitionKind propertyDefinitionKind, string value, Func<ICondition, IEnumerable<T>> sourceProvider) where T : class
    {
      var concept = _session.Objects.GetKnownConcept(conceptKind);
      var propDef = _session.Objects.GetKnownPropertyDefinition(propertyDefinitionKind);
      var condition = _session.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(value));
      return sourceProvider(condition).FirstOrDefault();
    }

    private IFormula CreateFormula(IFormulaGroup groupFormula, string name)
    {
      var formula = groupFormula.CreateFormula(name, FormulaDefaults.BuildFormulaBodyDesignation());
      foreach (var (Name, Expression) in FormulaDefaults.Parameters)
        formula.CreateParameter(Name, Expression);

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