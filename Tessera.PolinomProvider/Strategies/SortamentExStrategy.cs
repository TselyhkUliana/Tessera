using Ascon.Polynom.Api;
using System.Xml.Linq;
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
      var document = sortament.Documents.FirstOrDefault();
      group.LinkDocument(document);

      var conceptClassification = _apiHelper.GetConceptByAbsoluteCode(ConceptConstants.CONCEPT_CLASSIFICATION_ITEM);
      var conceptIntegration = _apiHelper.GetConceptByAbsoluteCode(ConceptConstants.CONCEPT_INTEGRATION);
      var conceptPropertySourceClassificationName = conceptClassification.ConceptPropertySources.FirstOrDefault(s => s.AbsoluteCode == PropConstants.PROP_NAME_AND_DESCRIPTION_ABSOLUTE_CODE);
      var conceptPropertySourceIntegrationKompas = conceptIntegration.ConceptPropertySources.FirstOrDefault(s => s.AbsoluteCode == PropConstants.PROP_INTEGRATION_KOMPAS3D_ABSOLUTE_CODE);
      var conceptPropertiesByStandard = AddConceptPropAccordingToStandart(element, EntityNameHelper.GetFullStandard(sortament.Name));
      var (formulaDesignation, formulaDesignationKOMPAS) = _apiHelper.CreateOrReceiveFormula(sortament.OwnerGroup.Name, $"Обозначение {groupName}", $"Обозначение в КОМПАС-3D {groupName}", conceptPropertiesByStandard);
      AssignFormulaToProperty(element, formulaDesignation, group, conceptPropertySourceClassificationName);
      AssignFormulaToProperty(element, formulaDesignationKOMPAS, group, conceptPropertySourceIntegrationKompas);
      
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

    private void AssignFormulaToProperty(IElement element, IFormula formula, IGroup group, IConceptPropertySource conceptPropertySource)
    {
      var prop = element.GetProperty(PropConstants.PROP_NAME_AND_DESCRIPTION);
      prop.EvaluationPropertyInfo.Formula = formula;
      var appointedFormula = group.AllAppointedFormulas.FirstOrDefault(af => af.Formula == formula) ??
                             group.AddAppointedFormula(conceptPropertySource, formula);
    }

    public void FillProperties()
    {

    }
  }
}
