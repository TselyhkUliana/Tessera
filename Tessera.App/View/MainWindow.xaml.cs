using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tessera.App.ViewModel;

namespace Tessera.App.View
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      DataContext = new MainWindowViewModel();
    }

    private void ComboBoxMaterial_Loaded(object sender, RoutedEventArgs e)
    {
      var comboBox = sender as ComboBox;
      comboBox.IsDropDownOpen = true;
    }

    private void ComboBoxProfiles_Loaded(object sender, RoutedEventArgs e)
    {
      var comboBox = sender as ComboBox;
      comboBox.IsDropDownOpen = true;
    }

    private void ComboBoxInstance_Loaded(object sender, RoutedEventArgs e)
    {
      var comboBox = sender as ComboBox;
      comboBox.IsDropDownOpen = true;
    }
  }
}