using Ascon.Polynom.Api;
using Tessera.App.PolinomHandlers.Utils;
using Tessera.App.ViewModel;

namespace Tessera.App.PolinomHandlers.Strategies
{
  internal class SortamentStrategy : ISortamentStrategy
  {
    private PolinomApiHelper _apiHelper;

    public SortamentStrategy(PolinomApiHelper polinomApiHelper)
    {
      _apiHelper = polinomApiHelper;
    }

    public IElement GetOrCreate(SectionDefinitionViewModel sectionDefinition)
    {
      var inputSortament = sectionDefinition.SectionProfile.Trim();
      var inputFirstWord = inputSortament.Split(' ')[0];
      var similarSortament = sectionDefinition.SuggestedProfiles
        .FirstOrDefault(x => x.Split(' ')[0].Equals(inputFirstWord, StringComparison.OrdinalIgnoreCase))?.Trim()
        ?? sectionDefinition.SuggestedInstances.First().Trim();

      var catalog = _apiHelper.GetCatalog(Constants.CATALOG_SORTAMENT);
      var searchElement = _apiHelper.SearchElement(similarSortament, Constants.CATALOG_SORTAMENT);

      if (similarSortament.Equals(inputSortament, StringComparison.OrdinalIgnoreCase))
        return searchElement;

      var inputElementFormat = EntityNameHelper.FormatFullName(inputSortament);
      var inputElementWordFirst = inputElementFormat.Split(' ')[0];
      var similarFirstWord = similarSortament.Split(' ')[0];
      var group = inputElementWordFirst.Equals(similarFirstWord, StringComparison.OrdinalIgnoreCase)
        ? searchElement.OwnerGroup
        : CreateGroupSortament(catalog.Groups.First(), inputElementWordFirst);

      var element = group.CreateElement(inputElementFormat);
      element.Applicability = Applicability.Allowed;
      FillProperties(inputElementFormat, inputElementWordFirst, group, element);
      return element;
    }

    private void FillProperties(string inputElementFormat, string inputElementWordFirst, IGroup group, IElement element)
    {
      AssignMarkProperty(inputElementFormat, element);

      var shapeProperty = element.GetProperty(Constants.PROP_SHAPE_MIS).Definition as IEnumPropertyDefinition;
      var conceptShape = _apiHelper.GetConcept(Constants.CONCEPT_SHAPE);
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
        //_apiHelper.ElementsForApproval.Add(element);
      }
      return formattedInputName;
    }

    private void AssignMarkProperty(string inputElementFormat, IElement element)
    {
      var markProperty = element.GetProperty(Constants.PROP_SORTAMENT_MASK).Definition as IStringPropertyDefinition;
      var conceptSortament = _apiHelper.GetConcept(Constants.CONCEPT_SORTAMENT);
      markProperty.AssignStringPropertyValue(element, conceptSortament, EntityNameHelper.GetNameBeforeStandard(inputElementFormat));
    }

    private IGroup CreateGroupSortament(IGroup parentGroup, string groupName)
    {
      var formulaGroups = _apiHelper.GetFormulaGroup();
      var groupFormula = _apiHelper.FindGroupByName(formulaGroups, g => g.FormulaGroups, Constants.GROUP_FORMULA_DESIGNATION_SORTAMENT);
      var formula = groupFormula.Formulas.FirstOrDefault(x => x.Name == Constants.FORMULA_DESIGNATION_SORTAMENT);
      var conceptClassification = _apiHelper.GetConcept(Constants.CONCEPT_CLASSIFICATION_ITEM);
      var group = parentGroup.CreateGroup(groupName);
      var conceptPropertySourceClassificationName = conceptClassification.ConceptPropertySources.FirstOrDefault(s => s.AbsoluteCode == Constants.PROP_NAME_AND_DESCRIPTION_ABSOLUTE_CODE);
      var appointedFormula = group.AddAppointedFormula(conceptPropertySourceClassificationName, formula);
      return group;
    }
  }
}