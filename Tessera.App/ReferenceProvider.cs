using Ascon.Polynom.Api;
using Ascon.Polynom.Login;
using System.Diagnostics;
using Tessera.App.ViewModel;

namespace Tessera.App
{
  internal class ReferenceProvider
  {
    private static ReferenceProvider _instance;
    private ISession _session;
    private IReference? _reference;
    private List<IElement> _elementsForApproval = new();

    private ReferenceProvider()
    {
      LoginManager.TryOpenSession(Guid.Parse(Constants.POLYNOM_CLIENT_ID), out _session);
      _reference = _session.Objects.AllReferences.FirstOrDefault(x => x.Name == Constants.REFENCE_NAME);
    }

    public static ReferenceProvider Instance => _instance ??= new ReferenceProvider();

    public void Start(IList<SectionDefinitionViewModel> sectionDefinitionViewModels)
    {
      var transaction = _session.Objects.StartTransaction();
      var sectionDefinition = sectionDefinitionViewModels.First();
      //var material = CreateOrReceiveMaterial(sectionDefinition);
      var sortament = CreateOrReceiveSortament(sectionDefinition);
      //var typeSize = CreateOrReceiveTypeSize(sectionDefinition, sortament.Name);
      //var sortamentEx = CreateSortamentEx(sortament);
      //CreateLink(sortament, material, Constants.LINK_SORTAMENT_MATERIAL);
      //CreateLink(typeSize, sortament, Constants.LINK_TYPESIZE_SORTAMENT);
      //CreateLink(sortamentEx, sortament, Constants.LINK_SORTAMENTEX_SORTAMENT);
      //CreateLink(sortamentEx, material, Constants.LINK_SORTAMENTEX_MATERIAL);
      //CreateLink(sortamentEx, typeSize, Constants.LINK_SORTAMENTEX_TYPE_SIZE);
      transaction.Commit();
      LinksTest(sortament);
    }

    public IElement CreateSortamentEx(IElement sortament)
    {
      var catalog = _reference.Catalogs.FirstOrDefault(x => x.Name == Constants.CATALOG_SORTAMENT_EX);
      var baseName = EntityNameHelper.GetNameBeforeStandard(sortament.Name);
      var index = EntityNameHelper.GetStandardKeywordIndex(sortament.Name);
      var standardPart = sortament.Name.Substring(index);
      var groupName = $"{baseName} {standardPart} {standardPart}"; //такой формат названия групп (например: Анод (золотой) ГОСТ 25475-2015 ГОСТ 25475-2015))
      var group = FindGroupByName(catalog.Groups, g => g.Groups, groupName) ?? CreateGroupSortamentEx(groupName, sortament.OwnerGroup.Name);
      var element = group.CreateElement("Element"); //название по умолчанию
      element.Applicability = Applicability.Allowed;
      var propName = element.GetProperty(Constants.PROP_NAME_AND_DESCRIPTION);
      var formula = CreateOrReceiveFormula(sortament.OwnerGroup.Name, $"Обозначение {groupName}");
      propName.EvaluationPropertyInfo.Formula = formula;
      var conceptClassification = _session.Objects.Get<IConcept>(Constants.CONCEPT_CLASSIFICATION_ITEM);
      var conceptPropertySourceClassificationName = conceptClassification.ConceptPropertySources.FirstOrDefault(s => s.AbsoluteCode == Constants.PROP_NAME_AND_DESCRIPTION_ABSOLUTE_CODE);
      var appointedFormula2 = group.AllAppointedFormulas.FirstOrDefault(af => af.Formula == formula) ??
                                     group.AddAppointedFormula(conceptPropertySourceClassificationName, formula);
      return element;
    }

    public IElement CreateOrReceiveTypeSize(SectionDefinitionViewModel sectionDefinition, string groupName)
    {
      var catalog = _reference.Catalogs.FirstOrDefault(x => x.Name == Constants.CATALOG_TYPE_SIZE);
      var group = FindGroupByName(catalog.Groups, g => g.Groups, groupName) ?? catalog.CreateGroup(groupName);
      var element = group.Elements.Where(e => e.Name.Equals(sectionDefinition.TypeSize, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      element ??= group.CreateElement(sectionDefinition.TypeSize);
      element.Applicability = Applicability.Allowed;
      return element;
    }

    private IElement CreateOrReceiveSortament(SectionDefinitionViewModel sectionDefinition)
    {
      var inputSortament = sectionDefinition.SectionProfile.Trim();
      var inputFirstWord = inputSortament.Split(' ')[0];
      var similarSortament = sectionDefinition.SuggestedProfiles
        .FirstOrDefault(x => x.Split(' ')[0].Equals(inputFirstWord, StringComparison.OrdinalIgnoreCase))?.Trim()
        ?? sectionDefinition.SuggestedInstances.First().Trim();

      var catalog = _reference.Catalogs.FirstOrDefault(c => c.Name == Constants.CATALOG_SORTAMENT);
      var searchElement = SearchElement(similarSortament, Constants.CATALOG_SORTAMENT);

      if (similarSortament.Equals(inputSortament, StringComparison.OrdinalIgnoreCase))
        return searchElement;

      var inputElementFormat = EntityNameHelper.FormatFullName(inputSortament);
      var inputElementWordFirst = inputElementFormat.Split(' ')[0];
      var similarFirstWord = similarSortament.Split(' ')[0];
      var group = inputElementWordFirst.Equals(similarFirstWord, StringComparison.OrdinalIgnoreCase)
        ? searchElement.OwnerGroup
        : catalog.Groups.First().CreateGroup(inputElementWordFirst);

      var element = group.CreateElement(inputElementFormat);
      element.Applicability = Applicability.Allowed;
      FillProperties(inputElementFormat, inputElementWordFirst, group, element);

      return element;
    }   

    public IElement CreateOrReceiveMaterial(SectionDefinitionViewModel sectionDefinition)
    {
      var inputMaterial = sectionDefinition.Material;
      var similarMaterial = sectionDefinition.SuggestedMaterials.First();
      var searchElement = SearchElement(similarMaterial, Constants.CATALOG_MATERIAL);

      if (similarMaterial.Equals(inputMaterial, StringComparison.OrdinalIgnoreCase))
        return searchElement;

      var inputElementFormat = EntityNameHelper.FormatFullName(inputMaterial);
      var group = searchElement.OwnerGroup;
      var element = group.CreateElement(inputElementFormat);
      element.Applicability = Applicability.Allowed;
      var markProperty = element.GetProperty(Constants.PROP_MATERIAL_MASK).Definition as IStringPropertyDefinition;
      var conceptMaterial = _session.Objects.Get<IConcept>(Constants.CONCEPT_MATERIAL);
      markProperty.AssignStringPropertyValue(element, conceptMaterial, EntityNameHelper.GetNameBeforeStandard(inputElementFormat));
      return element;
    }

    private IElement SearchElement(string similarElement, string catalogName)
    {
      var catalog = _reference.Catalogs.FirstOrDefault(c => c.Name == catalogName);
      var concept = _session.Objects.GetKnownConcept(KnownConceptKind.Element);
      var propDef = _session.Objects.GetKnownPropertyDefinition(KnownPropertyDefinitionKind.Name);
      var condition = _session.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(similarElement));
      var searchElement = catalog.Intersect(condition).GetEnumerable<IElement>().FirstOrDefault();
      return searchElement;
    }

    private void CreateLink(ILinkable left, ILinkable right, string aboluteCodeLink)
    {
      var group = _session.Objects.LinkDefCatalog.LinkDefGroups.FirstOrDefault(g => g.Name == Constants.REFENCE_NAME);
      var link = group.LinkDefinitions.FirstOrDefault(l => l.AbsoluteCode == aboluteCodeLink);
      link.Destination.CreateLink(left, right);
    }

    private static IFormula CreateNewFormula(IFormulaGroup groupFormula, string name)
    {
      var formula = groupFormula.CreateFormula(name, FormulaDefaults.BuildFormulaBodyDesignation());
      foreach (var (Name, Expression) in FormulaDefaults.Parameters)
        formula.CreateParameter(Name, Expression);

      return formula;
    }

    private static T FindGroupByName<T>(IApiReadOnlyCollection<T> groups, Func<T, IApiReadOnlyCollection<T>> childrenSelector, string groupName) where T : class, IBaseGroup
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

    private IFormula CreateOrReceiveFormula(string sortamentGroup, string formulaName)
    {
      var formulaGroups = _session.Objects.FormulaCatalog.FormulaGroups;
      var groupFormula = FindGroupByName(formulaGroups, g => g.FormulaGroups, sortamentGroup) ??
                         FindGroupByName(formulaGroups, g => g.FormulaGroups, "Обозначения экземпляров сортаментов").CreateFormulaGroup(sortamentGroup);
      var formula = groupFormula.Formulas.FirstOrDefault(x => x.Name == formulaName) ?? CreateNewFormula(groupFormula, formulaName);
      return formula;
    }

    private IGroup CreateGroupSortamentEx(string derivedGroupName, string baseGroupName)
    {
      var catalog = _reference.Catalogs.FirstOrDefault(x => x.Name == Constants.CATALOG_SORTAMENT_EX);
      var group = catalog.Groups.FirstOrDefault(x => x.Name == baseGroupName) ?? catalog.CreateGroup(baseGroupName);
      return group.CreateGroup(derivedGroupName);
    }

    private void FillProperties(string inputElementFormat, string inputElementWordFirst, IGroup group, IElement element)
    {
      AssignMarkProperty(inputElementFormat, element);

      var shapeProperty = element.GetProperty(Constants.PROP_SHAPE_MIS).Definition as IEnumPropertyDefinition;
      var conceptShape = _session.Objects.Get<IConcept>(Constants.CONCEPT_SHAPE);
      var formattedInputName = EntityNameHelper.FormatNameParts(inputElementFormat);

      var finalShapeValue = group.Elements.Count > 1
       ? GetShapeValueForMultipleElements(group, element, inputElementFormat, inputElementWordFirst, formattedInputName)
       : GetShapeValueForSingleElement(shapeProperty, formattedInputName, element);

      shapeProperty.AssignEnumPropertyValue(element, conceptShape, finalShapeValue);
    }

    private static string GetShapeValueForMultipleElements(IGroup group, IElement element, string inputElementFormat, string inputElementWordFirst, string formattedInputName)
    {
      var elements = group.Elements;

      var gostSuffix = EntityNameHelper.GetStandard(inputElementFormat);
      if (!string.IsNullOrEmpty(gostSuffix))
      {
        var gostShapeValue = (group.Elements.FirstOrDefault(e => e.Name.Contains(gostSuffix, StringComparison.OrdinalIgnoreCase))?
            .GetProperty(Constants.PROP_SHAPE_MIS) as IEnumPropertyValue)?.Value;
        if (!string.IsNullOrEmpty(gostShapeValue))
          return gostShapeValue;
      }

      var normalizedElements = elements.AsEnumerable()
          .Where(x => x.Id != element.Id)
          .Select(x => new
          {
            NormalizedName = EntityNameHelper.FormatNameParts(x.Name),
            ShapeValue = (x.GetPropertyValue(Constants.PROP_SHAPE_MIS) as IEnumPropertyValue)?.Value
          }).ToList();

      if (normalizedElements.All(x => x.ShapeValue is null))
        return null;
      if (normalizedElements.All(x => x.ShapeValue == normalizedElements[0].ShapeValue))
        return normalizedElements.First().ShapeValue;
      else if (normalizedElements.All(x => x.NormalizedName.Split(' ')[0] == x.ShapeValue))
        return inputElementWordFirst;
      if (normalizedElements.All(x => x.NormalizedName == x.ShapeValue))
        return formattedInputName;

      return null;
    }

    private string GetShapeValueForSingleElement(IEnumPropertyDefinition shapeProperty, string formattedInputName, IElement element)
    {
      if (!shapeProperty.Items.Any(s => s.Value == formattedInputName))
      {
        shapeProperty.AddItem(formattedInputName);
        _elementsForApproval.Add(element);
      }
      return formattedInputName;
    }

    private void AssignMarkProperty(string inputElementFormat, IElement element)
    {
      var markProperty = element.GetProperty(Constants.PROP_SORTAMENT_MASK).Definition as IStringPropertyDefinition;
      var conceptSortament = _session.Objects.Get<IConcept>(Constants.CONCEPT_SORTAMENT);
      markProperty.AssignStringPropertyValue(element, conceptSortament, EntityNameHelper.GetNameBeforeStandard(inputElementFormat));
    }

    //private void UpdateProperties()
    //{
    //  IAsyncOperation asyncOperation = _formula.EvaluateAsync();
    //  asyncOperation.Start();
    //  asyncOperation.Wait();
    //}

    private void LinksTest(IElement element)
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