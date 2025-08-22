using Ascon.Polynom.Api;
using System.Diagnostics;

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
      _referenceMaterialAndSortament = _session.Objects.AllReferences.FirstOrDefault(x => x.Name == Constants.REFENCE_NAME);
    }

    public IConcept GetConcept(string conceptCode) => _session.Objects.Get<IConcept>(conceptCode);

    public ICatalog GetCatalog(string catalogName) => _referenceMaterialAndSortament.Catalogs.FirstOrDefault(x => x.Name == catalogName);

    public IApiReadOnlyCollection<IFormulaGroup> GetFormulaGroups() => _session.Objects.FormulaCatalog.FormulaGroups

    public IElement SearchElement(string similarElement, string catalogName)
    {
      var catalog = GetCatalog(catalogName);
      var concept = _session.Objects.GetKnownConcept(KnownConceptKind.Element);
      var propDef = _session.Objects.GetKnownPropertyDefinition(KnownPropertyDefinitionKind.Name);
      var condition = _session.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(similarElement));
      var searchElement = catalog.Intersect(condition).GetEnumerable<IElement>().FirstOrDefault();
      return searchElement;
    }

    public IDocument SearchDocument(string standard)
    {
      var documentCatalog = _referenceMaterialAndSortament.DocumentCatalog;
      var concept = _session.Objects.GetKnownConcept(KnownConceptKind.Document);
      var propDef = _session.Objects.GetKnownPropertyDefinition(KnownPropertyDefinitionKind.Designation);
      var condition = _session.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(standard));
      var searchDocument = documentCatalog.Intersect(condition).GetEnumerable<IDocument>().FirstOrDefault();
      return searchDocument;
    }

    public void AddDocument(IElement element, string fullName, string documentGroupName)
    {
      var fullStandard = EntityNameHelper.GetFullStandard(fullName);
      var documentCatalog = _referenceMaterialAndSortament.DocumentCatalog;
      var documentGroup = documentCatalog.DocumentGroups.FirstOrDefault(x => x.Name == documentGroupName);
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

    public void CreateLink(ILinkable left, ILinkable right, string aboluteCodeLink)
    {
      var group = _session.Objects.LinkDefCatalog.LinkDefGroups.FirstOrDefault(g => g.Name == Constants.REFENCE_NAME);
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

      //var elementsName = elements.Select(e => EntityNameHelper.NormalizeName(e.Name)).ToList();
      //var elementsFirstWords = elementsName.Select(name => name.Split(' ')[0]).Distinct().ToList();
      //var matches = elementsShapeValue.Where(item => elementsFirstWords.Any(w => w.Equals(item, StringComparison.OrdinalIgnoreCase))).ToList();

      //var elementsShape = elements.AsEnumerable()
      //          .Select(e => (e.GetPropertyValue(Constants.PROP_SHAPE_MIS) as IEnumPropertyValue)?.Value)
      //          .Where(v => v != null)
      //          .Distinct()
      //          .ToList();
      //if (elementsShape.Count > 1)
      //{
    }
  }
}