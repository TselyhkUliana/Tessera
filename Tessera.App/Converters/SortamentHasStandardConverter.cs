using System.Globalization;
using System.Windows.Data;
using Tessera.App.PolinomProvider.Utils.Constants;

namespace Tessera.App.Converters
{
  public class SortamentHasStandardConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value switch
      {
        string sortament when !string.IsNullOrWhiteSpace(sortament)
            => StandardConstants.Standards.Any(x => sortament.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0),
        _ => true
      };


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
}
