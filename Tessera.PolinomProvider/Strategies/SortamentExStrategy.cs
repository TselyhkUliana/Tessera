using Ascon.Polynom.Api;
using Tessera.PolinomProvider.Constants;
using Tessera.PolinomProvider.Interface;
using Tessera.PolinomProvider.Utils;

namespace Tessera.PolinomProvider.Strategies
{
  internal class SortamentExStrategy : ISortamentExStrategy
  {
    private PolinomApiHelper _apiHelper;
    private readonly ICatalog _catalog;

    public SortamentExStrategy(PolinomApiHelper polinomApiHelper)
    {
      _apiHelper = polinomApiHelper;
      _catalog = _apiHelper.GetCatalog(CatalogConstants.CATALOG_SORTAMENT_EX);
    }

    public IElement GetOrCreate(IElement sortament, IElement material)
    {
      var baseName = EntityNameHelper.GetNameBeforeStandard(sortament.Name);
      var indexSortament = EntityNameHelper.GetStandardKeywordIndex(sortament.Name);
      var indexMaterial = EntityNameHelper.GetStandardKeywordIndex(material.Name);
      var sortamentStandardPart = sortament.Name.Substring(indexSortament);
      var materialStandardPart = material.Name.Substring(indexMaterial);
      var groupName = $"{baseName} {sortamentStandardPart} {materialStandardPart}"; //такой формат названия групп (например: Анод (золотой) ГОСТ 25475-2015 ГОСТ 25475-2015))
      
      var group = _apiHelper.FindGroupByName(_catalog.Groups, g => g.Groups, groupName) ?? CreateGroupSortamentEx(groupName, sortament.OwnerGroup.Name);
      var element = group.CreateElement(CatalogConstants.ELEMENT_DEFAULT_NAME);
      element.Applicability = Applicability.Allowed;
      var conceptClassification = _apiHelper.GetConceptByAbsoluteCode(ConceptConstants.CONCEPT_CLASSIFICATION_ITEM);
      var conceptPropertySourceClassificationName = conceptClassification.ConceptPropertySources.FirstOrDefault(s => s.AbsoluteCode == PropConstants.PROP_NAME_AND_DESCRIPTION_ABSOLUTE_CODE);
      var document = sortament.Documents.FirstOrDefault();
      group.LinkDocument(document);
      var conceptPropertiesByStandard = AddConceptPropAccordingToStandart(element, EntityNameHelper.GetFullStandard(sortament.Name));

      var formula = _apiHelper.CreateOrReceiveFormula(sortament.OwnerGroup.Name, $"Обозначение {groupName}", conceptPropertiesByStandard);
      var propName = element.GetProperty(PropConstants.PROP_NAME_AND_DESCRIPTION);
      propName.EvaluationPropertyInfo.Formula = formula;
      var appointedFormula = group.AllAppointedFormulas.FirstOrDefault(af => af.Formula == formula) ??
                             group.AddAppointedFormula(conceptPropertySourceClassificationName, formula);

      return element;
    }

    private IGroup CreateGroupSortamentEx(string derivedGroupName, string baseGroupName)
    {
      var group = _catalog.Groups.FirstOrDefault(x => x.Name == baseGroupName) ?? _catalog.CreateGroup(baseGroupName);
      return group.CreateGroup(derivedGroupName);
    }

    private IConcept AddConceptPropAccordingToStandart(IElement element, string standard)
    {
      IConcept result = null;
      _apiHelper.ExecuteWithEditorClient(() =>
      {
        var description = $"Свойства по {standard}";
        //var concept = _apiHelper.GetConceptByName(description) ??
        var concept = _apiHelper.GetConceptByName("Свойства по ГОСТ 1050-2013") ??
                      _apiHelper.GetConceptByAbsoluteCode(ConceptConstants.CONCEPT_SORTAMENT_EX).CreateSubConcept(description);
        var prop = concept.ConceptPropertySources.FirstOrDefault(x => x.AbsoluteCode == concept.AbsoluteCode + PropConstants.PROP_SPECIFICATION_OBJECT_SETTINGS_TEMPLATE);
        prop.IsDynamic = true;
        element.RealizeContract(concept);
        result = concept;
      });
      return result;
    }

    public void FillProperties()
    {

    }
  }
}
