using System.Text;
using static Tessera.PolinomProvider.Formula.PropertyGroupingService;

namespace Tessera.PolinomProvider.Formula
{
  internal class FormulaSectionBuilder
  {
    internal static void AppendExpressions(StringBuilder sb, IEnumerable<PropertyInfo> propps)
    {
      foreach (var item in propps)
        sb.Append(FormulaParts.Expression(item.Name));
    }
  }
}