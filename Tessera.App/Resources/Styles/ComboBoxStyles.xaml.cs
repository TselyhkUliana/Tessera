using System.Windows;
using System.Windows.Controls;

namespace Tessera.App.Resources.Styles
{
  public partial class ComboBoxStyles : ResourceDictionary
  {
    public ComboBoxStyles()
    {
      InitializeComponent();
    }

    private void ComboBoxMaterial_Loaded(object sender, RoutedEventArgs e)
    {
      var comboBox = sender as ComboBox;
      comboBox.IsDropDownOpen = true;
      comboBox.Focus();
    }

    private void ComboBoxSortament_Loaded(object sender, RoutedEventArgs e)
    {
      var comboBox = sender as ComboBox;
      comboBox.IsDropDownOpen = true;
      comboBox.Focus();
    }
  }
}
