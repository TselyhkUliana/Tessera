using Ascon.Polynom.Api;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tessera.App.Polinom.Utils;
using Tessera.App.Polinom.Utils.Constants;
using Tessera.App.ViewModel;

namespace Tessera.App.Polinom.Strategies
{
  internal class TypeSizeStrategy : ITypeSizeStrategy
  {
    private PolinomApiHelper _apiHelper;

    public TypeSizeStrategy(PolinomApiHelper polinomApiHelper)
    {
      _apiHelper = polinomApiHelper;
    }

    public IElement GetOrCreate(TypeSizeViewModel typeSizeViewModel, IElement sortament)
    {
      var catalog = _apiHelper.GetCatalog(CatalogConstants.CATALOG_TYPE_SIZE);
      var groupName = sortament.Name;
      var group = _apiHelper.FindGroupByName(catalog.Groups, g => g.Groups, groupName) ?? catalog.CreateGroup(groupName);
      var element = FindMatchingElement(group.Elements, typeSizeViewModel) ?? CreateElementWithProperties(group, typeSizeViewModel, sortament);
      element.Applicability = Applicability.Allowed;
      return element;
    }

    public IElement FindMatchingElement(IApiReadOnlyCollection<IElement> elements, TypeSizeViewModel typeSizeViewModel)
    {
      var matchingElements = elements.Where(e => e.Name.Equals(typeSizeViewModel.TypeSize, StringComparison.OrdinalIgnoreCase)).ToList();
      var elementsWithProperties = matchingElements.Select(element =>
      {
        var properties = element.Properties
        .Where(x => x.Contract.AbsoluteCode.Contains(ConceptConstants.REQUIREMENT_CONCEPT))
        .Select(p => new
        {
          Name = p.Definition.Name,
          Value = GetValue(p)
        });

        return new { Element = element, Properties = properties };
      }).ToList();
      var inputProperties = typeSizeViewModel.TypeSizePropertiesViewModel.Where(p => !string.IsNullOrEmpty(p.Value));
      var filteredElement = elementsWithProperties.FirstOrDefault(e =>
      {
        var propertyDict = e.Properties.ToDictionary(p => p.Name, p => p.Value);
        return inputProperties.All(nv => propertyDict.TryGetValue(nv.Name, out var value) && value == nv.Value);
      });
      return filteredElement?.Element;
    }

    public IElement CreateElementWithProperties(IGroup group, TypeSizeViewModel typeSizeViewModel, IElement sortament)
    {
      var element = group.CreateElement(typeSizeViewModel.TypeSize);
      FillProperties(typeSizeViewModel.TypeSizePropertiesViewModel, sortament, element, group);
      return element;
    }

    public void FillProperties(ObservableCollection<TypeSizePropertyViewModel> typeSizeProperties, IElement sortament, IElement typeSize, IGroup group)
    {
      var inputProperties = typeSizeProperties.Where(p => !string.IsNullOrEmpty(p.Value)).ToList();
      var inputPropertiesName = inputProperties.Select(x => x.Name).ToList();
      if (!inputProperties.Any())
        return;
      var conceptShape = _apiHelper.GetConceptByAbsoluteCode(ConceptConstants.CONCEPT_SORTAMENT);
      var sharpProperty = (sortament.GetProperty(PropConstants.PROP_MATERIALS_MARK).Value as IStringPropertyValue).Value;
      _apiHelper.ExecuteWithEditorClient(() =>
      {
        var concept = group.Elements.Count > 1
                      ? typeSize.AllContracts.FirstOrDefault(x => x.AbsoluteCode.Contains(ConceptConstants.REQUIREMENT_CONCEPT))
                      : _apiHelper.GetConceptByAbsoluteCode(ConceptConstants.REQUIREMENT_CONCEPT).CreateSubConcept($"{sharpProperty}.Размерность");
        var common = _apiHelper.PropertyDefinitions.Where(x => inputPropertiesName.Contains(x.Name)).ToList();
        foreach (var property in common)
        {
          switch (property.Type)
          {
            case Ascon.Polynom.Api.PropertyType.Double:
              var propDouble = property as IDoublePropertyDefinition;
              propDouble.AssignDoublePropertyValue(typeSize, concept, double.Parse(inputProperties.First(x => x.Name == property.Name).Value));
              break;
            case Ascon.Polynom.Api.PropertyType.String:
              var propString = property as IStringPropertyDefinition;
              propString.AssignStringPropertyValue(typeSize, concept, inputProperties.First(x => x.Name == property.Name).Value);
              break;
            default:
              break;
          }
          //var propertySource = concept.CreatePropertySource(property);
          //switch (propertySource.Definition.Type)
          //{
          //  case Ascon.Polynom.Api.PropertyType.Double:
          //    var propDouble = propertySource.Definition as IDoublePropertyDefinition;
          //    propDouble.AssignDoublePropertyValue(typeSize, concept, double.Parse(inputProperties.First(x => x.Name == property.Name).Value));
          //    break;
          //  case Ascon.Polynom.Api.PropertyType.String:
          //    var propString = propertySource.Definition as IStringPropertyDefinition;
          //    propString.AssignStringPropertyValue(typeSize, concept, inputProperties.First(x => x.Name == property.Name).Value);
          //    break;
          //  default:
          //    break;
          //}
        }
      });
    }

    public string GetValue(IProperty property)
    {
      return property.Definition.Type switch
      {
        Ascon.Polynom.Api.PropertyType.String => (property.Value as IStringPropertyValue)?.Value,
        Ascon.Polynom.Api.PropertyType.Double => (property.Value as IDoublePropertyValue)?.Value.ToString(),
        _ => string.Empty
      };
    }
  }
}