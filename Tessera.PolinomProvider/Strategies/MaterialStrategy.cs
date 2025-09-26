using Ascon.Polynom.Api;
using Tessera.PolinomProvider.Constants;
using Tessera.PolinomProvider.Model;
using Tessera.PolinomProvider.Utils;

namespace Tessera.PolinomProvider.Strategies
{
  internal class MaterialStrategy : IMaterialStrategy
  {
    private PolinomApiHelper _apiHelper;

    public MaterialStrategy(PolinomApiHelper polinomApiHelper)
    {
      _apiHelper = polinomApiHelper;
    }

    public IElement GetOrCreate(SectionDefinition sectionDefinition)
    {
      var inputMaterial = sectionDefinition.Material;
      var similarMaterial = sectionDefinition.SuggestedMaterials.First();
      var searchElement = _apiHelper.SearchElement(similarMaterial, CatalogConstants.CATALOG_MATERIAL);

      if (similarMaterial.Equals(inputMaterial, StringComparison.OrdinalIgnoreCase))
      {
        searchElement.Applicability = Applicability.Allowed;
        return searchElement;
      }

      var inputElementFormat = EntityNameHelper.FormatFullName(inputMaterial);
      var group = searchElement.OwnerGroup;
      var element = group.CreateElement(CatalogConstants.ELEMENT_DEFAULT_NAME);
      element.Applicability = Applicability.Allowed;
      FillProperties(element, inputElementFormat);
      _apiHelper.AddOrCreateDocument(element, inputElementFormat, CatalogConstants.GROUP_DOCUMENT_MATERIAL);
      element.Evaluate();
      return element;
    }

    public void FillProperties(IElement element, string inputElementFormat)
    {
      var markProperty = element.GetProperty(PropConstants.PROP_MATERIAL_MASK).Definition as IStringPropertyDefinition;
      var conceptMaterial = _apiHelper.GetConceptByAbsoluteCode(ConceptConstants.CONCEPT_MATERIAL);
      markProperty.AssignStringPropertyValue(element, conceptMaterial, EntityNameHelper.GetNameBeforeStandard(inputElementFormat));
    }
  }
}
