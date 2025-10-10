using Ascon.Polynom.Api;
using System.Text;

namespace Tessera.PolinomProvider.Utils
{
  internal static class FormulaBuilder
  {
    private static PropertyList _properties;
    private static PropertyList Properties => _properties ??= SerializerProp.Instance;
    private static List<(string name, string category, int priority)> _propertyInfo;

    public static void BuildFormulaBodyDesignationAndKOMPAS(IConcept conceptPropertiesByStandard)
    {
      BuildFormulaBodyParameters(conceptPropertiesByStandard);
      var formulaBody = BuildFormulaBodyDesignation();
    }

    public static void BuildFormulaBodyParameters(IConcept conceptPropertiesByStandard)
    {
      _propertyInfo = new();
      foreach (var conceptPropertySource in conceptPropertiesByStandard.ConceptPropertySources)
      {
        var definition = conceptPropertySource.GetPropertyDefinition();
        var property = Properties.Items.FirstOrDefault(x => x.Name == definition.Name);
        _propertyInfo.Add(($"{definition.Name}.{definition.OwnerGroup.Name}", property.Category, property.Priority));
      }
    }

    /// <summary>Возвращает тело формулы для вычисления обозначения экземпляра сортамента</summary>
    public static string BuildFormulaBodyDesignation()
    {
      var formulaBody = string.Empty;
      var basicPriority = _propertyInfo.Where(x => x.category == "Basic" && x.priority == 1);
      var basic = _propertyInfo.Where(x => x.category == "Basic" && x.priority != 1);
      var geometry = _propertyInfo.Where(x => x.category == "Geometry");
      var mechanicAndSurface = _propertyInfo.Where(x => (x.category == "Mechanic" || x.category == "Surface") && x.priority != 1);
      var mechanicAndSurfacePriority = _propertyInfo.Where(x => (x.category == "Mechanic" || x.category == "Surface") && x.priority == 1);
      var additional = _propertyInfo.Where(x => x.category == "Additional" && x.priority != 1);
      var additionalPriority = _propertyInfo.Where(x => x.category == "Additional" && x.priority == 1);
      if (basicPriority.Any())
      {
        formulaBody += BuildFormulaBodyBasicPriority(basicPriority.First().name);
        formulaBody += BuildFormulaBodyBasicSortament();
        formulaBody += BuildFormulaBodyEnd();
        formulaBody += ", '', ' ')";
      }
      else
      {
        formulaBody += "StringTrimStart(ToString(if(IsNull([Сортамент.Форма]), '', [Сортамент.Форма])\r\n ";
      }
      if (basic.Any())
      {
        foreach (var parameter in basic)
          formulaBody += BuildFormulaBodyExpression(parameter.name);
      }
      if (geometry.Any())
      {
        foreach (var parameter in geometry)
          formulaBody += BuildFormulaBodyExpression(parameter.name);
      }
      if (mechanicAndSurfacePriority.Any())
      {
        foreach (var parameter in mechanicAndSurfacePriority)
          formulaBody += BuildFormulaBodyExpression(parameter.name);
      }
      formulaBody += BuildFormulaBodyTypeSize();
      formulaBody += BuildFormulaBodyMaterial();
      if (mechanicAndSurface.Any())
      {
        foreach (var parameter in mechanicAndSurface)
          formulaBody += BuildFormulaBodyExpression(parameter.name);
      }
      if (additional.Any())
      {
        foreach (var parameter in additional)
          formulaBody += BuildFormulaBodyExpression(parameter.name);
      }
      formulaBody += BuildFormulaBodyStandard();
      formulaBody += BuildFormulaBodyEnd();
      if (additionalPriority.Any())
      {
        formulaBody += BuildFormulaBodyAdditionalPriority(additionalPriority.First().name);
      }

      return formulaBody;
    }

    public static string BuildFormulaBodyBasicPriority(string parameter)
    {
      return $"StringPrefixSuffix(StringTrimStart(ToString(if(IsNull([{parameter}]), '', [{parameter}])\r\n";
    }

    public static string BuildFormulaBodyBasicSortament()
    {
      return $"+ StringPrefixSuffix([Сортамент.Форма], ' ', '')\r\n";
    }

    public static string BuildFormulaBodyEnd()
    {
      return "), StringArray(' '))";
    }

    public static string BuildFormulaBodyExpression(string parameter)
    {
      return $"+ StringPrefixSuffix([{parameter}], '-', '')\r\n";
    }

    public static string BuildFormulaBodyAdditionalPriority(string parameter)
    {
      return $"+ StringPrefixSuffix(StringTrimStart(ToString(StringPrefixSuffix([{parameter}], ' ', '')\r\n), StringArray(' '))\r\n, ' ', '')";
    }

    public static string BuildFormulaBodyTypeSize()
    {
      return "+ StringPrefixSuffix([Типоразмер.Наименование], ' ', '')\r\n";
    }

    public static string BuildFormulaBodyMaterial()
    {
      return "+ StringPrefixSuffix([Материал.Марка], ' ', '')\r\n";
    }

    public static string BuildFormulaBodyStandard()
    {
      return "+ StringPrefixSuffix([ТУ.Документ], ' ', '')\r\n";
    }
  }
}
