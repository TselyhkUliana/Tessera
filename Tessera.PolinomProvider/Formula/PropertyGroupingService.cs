using Ascon.Polynom.Api;

namespace Tessera.PolinomProvider.Formula
{
  internal static class PropertyGroupingService
  {
    private static PropertyList _properties;
    private static PropertyList Properties => _properties ??= SerializerProp.Instance;
    internal record PropertyInfo(string Name, string Expression, PropertyCategory Category, int Priority);

    internal static List<PropertyInfo> GetProperties(IConcept conceptPropertiesByStandard)
    {
      var result = new List<PropertyInfo>();
      foreach (var conceptPropertySource in conceptPropertiesByStandard.ConceptPropertySources)
      {
        var definition = conceptPropertySource.GetPropertyDefinition();
        var property = Properties.Items.FirstOrDefault(x => x.Name == definition.Name);
        Enum.TryParse<PropertyCategory>(property.Category, out var category);
        result.Add(new PropertyInfo($"{definition.Name}.{definition.OwnerGroup.Name}", 
                  $"GetPropertyValue([this], '{conceptPropertySource.AbsoluteCode}', '')", 
                  category, 
                  property.Priority));
      }
      return result;
    }

    internal static Dictionary<PropertyCategory, List<PropertyInfo>> GroupByCategory(List<PropertyInfo> propertyInfos)
    {
      return propertyInfos.GroupBy(x => x.Category).ToDictionary(x => x.Key, g => g.ToList());
    }
  }
}
