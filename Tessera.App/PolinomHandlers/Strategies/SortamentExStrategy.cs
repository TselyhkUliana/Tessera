using Ascon.Polynom.Api;
using System.Reflection.Metadata;
using Tessera.App.PolinomHandlers.Utils;

namespace Tessera.App.PolinomHandlers.Strategies
{
  internal class SortamentExStrategy : ISortamentExStrategy
  {
    private PolinomApiHelper _apiHelper;
    private readonly ICatalog _catalog;

    public SortamentExStrategy(PolinomApiHelper polinomApiHelper)
    {
      _apiHelper = polinomApiHelper;
      _catalog = _apiHelper.GetCatalog(Constants.CATALOG_SORTAMENT_EX);
    }

    public IElement GetOrCreate(IElement sortament)
    {
      var baseName = EntityNameHelper.GetNameBeforeStandard(sortament.Name);
      var index = EntityNameHelper.GetStandardKeywordIndex(sortament.Name);
      var standardPart = sortament.Name.Substring(index);
      var groupName = $"{baseName} {standardPart} {standardPart}"; //такой формат названия групп (например: Анод (золотой) ГОСТ 25475-2015 ГОСТ 25475-2015))
      var group = _apiHelper.FindGroupByName(_catalog.Groups, g => g.Groups, groupName) ?? CreateGroupSortamentEx(groupName, sortament.OwnerGroup.Name);
      var element = group.CreateElement(Constants.ELEMENT_DEFAULT_NAME);
      element.Applicability = Applicability.Allowed;
      var propName = element.GetProperty(Constants.PROP_NAME_AND_DESCRIPTION);
      var formula = _apiHelper.CreateOrReceiveFormula(sortament.OwnerGroup.Name, $"Обозначение {groupName}", Constants.GROUP_FORMULA_DESIGNATION_SORTAMENT_EX);
      propName.EvaluationPropertyInfo.Formula = formula;
      var conceptClassification = _apiHelper.GetConceptByAbsoluteCode(Constants.CONCEPT_CLASSIFICATION_ITEM);
      var conceptPropertySourceClassificationName = conceptClassification.ConceptPropertySources.FirstOrDefault(s => s.AbsoluteCode == Constants.PROP_NAME_AND_DESCRIPTION_ABSOLUTE_CODE);
      var appointedFormula = group.AllAppointedFormulas.FirstOrDefault(af => af.Formula == formula) ??
                             group.AddAppointedFormula(conceptPropertySourceClassificationName, formula);
      var document = sortament.Documents.FirstOrDefault();
      group.LinkDocument(document);
      AddConceptPropAccordingToStandart(element, EntityNameHelper.GetFullStandard(sortament.Name));
      
      return element;
    }

    private IGroup CreateGroupSortamentEx(string derivedGroupName, string baseGroupName)
    {
      var group = _catalog.Groups.FirstOrDefault(x => x.Name == baseGroupName) ?? _catalog.CreateGroup(baseGroupName);
      return group.CreateGroup(derivedGroupName);
    }

    private void AddConceptPropAccordingToStandart(IElement element, string standard)
    {
      var description = $"Свойства по {standard}";
      var concept = _apiHelper.GetConceptByName(description) ??
                    _apiHelper.GetConceptByAbsoluteCode(Constants.CONCEPT_SORTAMENT_EX).CreateSubConcept(description);
      var prop = concept.GetConceptPropertySource(Constants.PROP_SPECIFICATION_OBJECT_SETTINGS_TEMPLATE);
      prop.IsDynamic = true;
      element.RealizeContract(concept);
    }

    public void FillProperties()
    {

    }
  }
}
