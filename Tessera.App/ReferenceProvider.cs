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
      var material = CreateOrReceiveMaterial(sectionDefinition);
      var sortament = CreateOrReceiveSortament(sectionDefinition);
      var typeSize = CreateOrReceiveTypeSize(sectionDefinition, sortament.Name);
      var sortamentEx = CreateSortamentEx(sortament);
      CreateLink(sortament, material, Constants.LINK_SORTAMENT_MATERIAL);
      CreateLink(typeSize, sortament, Constants.LINK_TYPESIZE_SORTAMENT);
      CreateLink(sortamentEx, sortament, Constants.LINK_SORTAMENTEX_SORTAMENT);
      CreateLink(sortamentEx, material, Constants.LINK_SORTAMENTEX_MATERIAL);
      CreateLink(sortamentEx, typeSize, Constants.LINK_SORTAMENTEX_TYPE_SIZE);
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
      var group = FindGroupByName<IGroup>(catalog.Groups, g => g.Groups, groupName) ?? CreateGroupSortamentEx(groupName, sortament.OwnerGroup.Name);
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

    public IFormula CreateOrReceiveFormula(string sortamentGroup, string formulaName)
    {
      var formulaGroups = _session.Objects.FormulaCatalog.FormulaGroups;
      var groupFormula = FindGroupByName<IFormulaGroup>(formulaGroups, g => g.FormulaGroups, sortamentGroup) ??
                         FindGroupByName<IFormulaGroup>(formulaGroups, g => g.FormulaGroups, "Обозначения экземпляров сортаментов").CreateFormulaGroup(sortamentGroup);
      var formula = groupFormula.Formulas.FirstOrDefault(x => x.Name == formulaName) ?? CreateNewFormula(groupFormula, formulaName);
      return formula;
    }

    public IElement CreateOrReceiveTypeSize(SectionDefinitionViewModel sectionDefinition, string groupName)
    {
      var catalog = _reference.Catalogs.FirstOrDefault(x => x.Name == Constants.CATALOG_TYPE_SIZE);
      var group = FindGroupByName<IGroup>(catalog.Groups, g => g.Groups, groupName) ?? catalog.CreateGroup(groupName);
      var element = group.Elements.Where(e => e.Name.Equals(sectionDefinition.TypeSize, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      element ??= group.CreateElement(sectionDefinition.TypeSize);
      element.Applicability = Applicability.Allowed;
      return element;
    }

    private IElement CreateOrReceiveSortament(SectionDefinitionViewModel sectionDefinition)
    {
      var inputSortament = sectionDefinition.SectionProfile.Trim();
      var similarSortament = sectionDefinition.SuggestedProfiles.First().Trim();
      var catalog = _reference.Catalogs.FirstOrDefault(c => c.Name == Constants.CATALOG_SORTAMENT);
      var concept = _session.Objects.GetKnownConcept(KnownConceptKind.Element);
      var propDef = _session.Objects.GetKnownPropertyDefinition(KnownPropertyDefinitionKind.Name);
      var simpleCondition = _session.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(similarSortament));
      var searchElement = SearchElement(similarSortament, Constants.CATALOG_SORTAMENT);

      if (similarSortament.Equals(inputSortament, StringComparison.OrdinalIgnoreCase))
        return searchElement;

      var inputElementFormat = inputSortament.FirstCharToUpper();
      var inputElementWords = inputElementFormat.Split(' ');
      var simularElementWords = similarSortament.Split(' ');
      var inputElementWordFirst = inputElementWords[0];
      var group = inputElementWordFirst == simularElementWords[0]
        ? searchElement.OwnerGroup
        : catalog.Groups.First().CreateGroup(inputElementWordFirst.FirstCharToUpper());

      var element = group.CreateElement(inputElementFormat);
      element.Applicability = Applicability.Allowed;
      var markProperty = element.GetProperty(Constants.PROP_SORTAMENT_MASK).Definition as IStringPropertyDefinition;
      var conceptSortament = _session.Objects.Get<IConcept>(Constants.CONCEPT_SORTAMENT);
      var conceptShape = _session.Objects.Get<IConcept>(Constants.CONCEPT_SHAPE);
      markProperty.AssignStringPropertyValue(element, conceptSortament, EntityNameHelper.GetNameBeforeStandard(inputElementFormat));
      return element;
    }

    private IElement CreateOrReceiveMaterial(SectionDefinitionViewModel sectionDefinition)
    {
      var inputMaterial = sectionDefinition.Material;
      var similarMaterial = sectionDefinition.SuggestedMaterials.First();
      var searchElement = SearchElement(similarMaterial, Constants.CATALOG_MATERIAL);

      if (similarMaterial.Equals(inputMaterial, StringComparison.OrdinalIgnoreCase))
        return searchElement;

      var inputElementFormat = inputMaterial.FirstCharToUpper();
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

    public void CreateLink(ILinkable left, ILinkable right, string aboluteCodeLink)
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

    public static T FindGroupByName<T>(IApiReadOnlyCollection<T> groups, Func<T, IApiReadOnlyCollection<T>> childrenSelector, string groupName) where T : class, IBaseGroup
    {
      foreach (var item in groups)
      {
        if (item.Name == groupName)
          return item;
        var found = FindGroupByName(childrenSelector(item), childrenSelector, groupName);
        if (found is not null)
          return found;
      }
      return null;
    }

    public IGroup CreateGroupSortamentEx(string derivedGroupName, string baseGroupName)
    {
      var catalog = _reference.Catalogs.FirstOrDefault(x => x.Name == Constants.CATALOG_SORTAMENT_EX);
      var group = catalog.Groups.FirstOrDefault(x => x.Name == baseGroupName) ?? catalog.CreateGroup(baseGroupName);
      return group.CreateGroup(derivedGroupName);
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
    }
  }
}