using Ascon.Polynom.Api;
using System.Text;
using static Tessera.PolinomProvider.Formula.PropertyGroupingService;

namespace Tessera.PolinomProvider.Formula
{
  internal class FormulaBuilder
  {
    private record CategorizedProperties(
      IEnumerable<PropertyInfo> Basic,
      IEnumerable<PropertyInfo> BasicPriority,
      IEnumerable<PropertyInfo> Geometry,
      IEnumerable<PropertyInfo> Surface,
      IEnumerable<PropertyInfo> SurfacePriority,
      IEnumerable<PropertyInfo> Mechanics,
      IEnumerable<PropertyInfo> MechanicsPriority,
      IEnumerable<PropertyInfo> Additional,
      IEnumerable<PropertyInfo> AdditionalPriority);

    internal record FormulaResult(Lazy<string> FormulaBodyDesignation, Lazy<string> FormulaBodyKOMPAS, List<(string name, string expression)> UsedParameters);

    internal static FormulaResult BuildFormulaBodyDesignationAndKOMPAS(IConcept conceptPropertiesByStandard)
    {
      var properties = GetProperties(conceptPropertiesByStandard);
      var groupedProperties = GroupByCategory(properties);
      var categorizedProperties = GetCategorizedProperties(groupedProperties);
      var lazyFormulaBodyDesignation = new Lazy<string>(() => BuildFormulaBody(categorizedProperties, false));
      var lazyFormulaBodyKOMPAS = new Lazy<string>(() => BuildFormulaBody(categorizedProperties, true));
      var usedParameters = CollectUsedParameters(categorizedProperties);

      return new FormulaResult(lazyFormulaBodyDesignation, lazyFormulaBodyKOMPAS, usedParameters);
    }

    private static CategorizedProperties GetCategorizedProperties(Dictionary<PropertyCategory, List<PropertyInfo>> grouped)
    {
      var basic = grouped.GetValueOrDefault(PropertyCategory.Basic) ?? new List<PropertyInfo>();
      var geometry = grouped.GetValueOrDefault(PropertyCategory.Geometry) ?? new List<PropertyInfo>();
      var surface = grouped.GetValueOrDefault(PropertyCategory.Surface) ?? new List<PropertyInfo>();
      var surfacePriority = surface.Where(x => x.Priority == 1);
      var mechanics = grouped.GetValueOrDefault(PropertyCategory.Mechanics) ?? new List<PropertyInfo>();
      var additional = grouped.GetValueOrDefault(PropertyCategory.Additional) ?? new List<PropertyInfo>();

      return new CategorizedProperties(
        basic.Where(x => x.Priority is not 1),
        basic.Where(x => x.Priority is 1),
        geometry,
        surface.Where(x => x.Priority is not 1),
        surface.Where(x => x.Priority is 1),
        mechanics.Where(x => x.Priority is not 1),
        mechanics.Where(x => x.Priority is 1),
        additional.Where(x => x.Priority is not 1),
        additional.Where(x => x.Priority is 1));
    }

    private static string BuildFormulaBody(CategorizedProperties categorizedProperties, bool kompas)
    {
      var b = new StringBuilder();

      if (categorizedProperties.BasicPriority.Any())
      {
        b.Append(FormulaParts.BasicPriority(categorizedProperties.BasicPriority.First().Name));
        FormulaSectionBuilder.AppendExpressions(b, categorizedProperties.BasicPriority.Skip(1));
        b.Append(FormulaParts.Sortament());
        b.Append(FormulaParts.End());
        b.Append(", '', ' ')");
      }
      else
      {
        b.Append("StringTrimStart(ToString(if(IsNull([Сортамент.Форма]), '', [Сортамент.Форма])\r\n ");
      }

      if (kompas)
        b.Append("+ '$d' ");

      FormulaSectionBuilder.AppendExpressions(b, categorizedProperties.Basic);
      FormulaSectionBuilder.AppendExpressions(b, categorizedProperties.Geometry);
      FormulaSectionBuilder.AppendExpressions(b, categorizedProperties.SurfacePriority);
      FormulaSectionBuilder.AppendExpressions(b, categorizedProperties.MechanicsPriority);

      b.Append(FormulaParts.TypeSize());

      if (kompas)
        b.Append("+ ';' + StringTrimStart(ToString([Материал.Марка]\r\n");
      else
        b.Append(FormulaParts.Material());

      FormulaSectionBuilder.AppendExpressions(b, categorizedProperties.Surface);
      FormulaSectionBuilder.AppendExpressions(b, categorizedProperties.Mechanics);
      FormulaSectionBuilder.AppendExpressions(b, categorizedProperties.Additional);

      b.Append(FormulaParts.Standard());
      b.Append(FormulaParts.End());

      if (kompas)
        b.Append(" + '$'");

      if (categorizedProperties.AdditionalPriority.Any())
        b.Append(FormulaParts.AdditionalPriority(categorizedProperties.AdditionalPriority.First().Name));

      return b.ToString();
    }

    private static List<(string name, string expression)> CollectUsedParameters(CategorizedProperties categorizedProperties)
    {
      var usedParameters = new List<(string name, string expression)>();
      var defaultParameters = FormulaParts.Parameters;
      usedParameters.AddRange(categorizedProperties.BasicPriority.Select(x => (x.Name, x.Expression)));
      usedParameters.Add((defaultParameters[0].name, defaultParameters[0].expression));
      usedParameters.AddRange(categorizedProperties.Basic.Select(x => (x.Name, x.Expression)));
      usedParameters.AddRange(categorizedProperties.Geometry.Select(x => (x.Name, x.Expression)));
      usedParameters.AddRange(categorizedProperties.SurfacePriority.Select(x => (x.Name, x.Expression)));
      usedParameters.AddRange(categorizedProperties.MechanicsPriority.Select(x => (x.Name, x.Expression)));
      usedParameters.Add((defaultParameters[1].name, defaultParameters[1].expression));
      usedParameters.AddRange(categorizedProperties.Surface.Select(x => (x.Name, x.Expression)));
      usedParameters.AddRange(categorizedProperties.Mechanics.Select(x => (x.Name, x.Expression)));
      usedParameters.AddRange(categorizedProperties.Additional.Select(x => (x.Name, x.Expression)));
      usedParameters.Add((defaultParameters[2].name, defaultParameters[2].expression));
      usedParameters.Add((defaultParameters[3].name, defaultParameters[3].expression));
      usedParameters.AddRange(categorizedProperties.AdditionalPriority.Select(x => (x.Name, x.Expression)));
      return usedParameters;
    }
  }
}