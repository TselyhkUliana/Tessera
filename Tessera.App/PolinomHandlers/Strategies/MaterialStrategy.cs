using Ascon.Polynom.Api;
using Tessera.App.PolinomHandlers.Utils;
using Tessera.App.ViewModel;

namespace Tessera.App.PolinomHandlers.Strategies
{
  internal class MaterialStrategy : IMaterialStrategy
  {
    private PolinomApiHelper _apiHelper;

    public MaterialStrategy(PolinomApiHelper polinomApiHelper)
    {
      _apiHelper = polinomApiHelper;
    }

    public IElement GetOrCreate(SectionDefinitionViewModel sectionDefinition)
    {
      var inputMaterial = sectionDefinition.Material;
      var similarMaterial = sectionDefinition.SuggestedMaterials.First();
      var searchElement = _apiHelper.SearchElement(similarMaterial, Constants.CATALOG_MATERIAL);

      if (similarMaterial.Equals(inputMaterial, StringComparison.OrdinalIgnoreCase))
        return searchElement;

      var inputElementFormat = EntityNameHelper.FormatFullName(inputMaterial);
      var group = searchElement.OwnerGroup;
      var element = group.CreateElement(inputElementFormat);
      element.Applicability = Applicability.Allowed;
      FillProperties(element, inputElementFormat);
      return element;
    }

    public void FillProperties(IElement element, string inputElementFormat)
    {
      var markProperty = element.GetProperty(Constants.PROP_MATERIAL_MASK).Definition as IStringPropertyDefinition;
      var conceptMaterial = _apiHelper.GetConcept(Constants.CONCEPT_MATERIAL);
      markProperty.AssignStringPropertyValue(element, conceptMaterial, EntityNameHelper.GetNameBeforeStandard(inputElementFormat));
    }
  }
}
