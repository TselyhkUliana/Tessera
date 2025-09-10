using System.Globalization;
using System.Windows.Data;
using Tessera.App.Polinom.Utils.Constants;

namespace Tessera.App.Converters
{
  public class MaterialHasStandardConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value switch
      {
        string material when !string.IsNullOrWhiteSpace(material)
            => StandardConstants.Standards.Any(x => material.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0),
        _ => true
      };


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
}
