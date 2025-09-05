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
      DataContext = new MainWindowViewModel(PolinomProvider.PolinomProvider.Instance);
    }

    private void MyGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
      if (DataContext is MainWindowViewModel vm)
        vm.CurrentSection.IsTypeSizeEditing = e.Column.Header.ToString() == "Типоразмеры";
    }

    private void MyGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
      //подумать что сделать чтобы после потери фокуса с ячейки редактирования не сбрасывался IsTypeSizeEditing
      if (DataContext is MainWindowViewModel vm)
        vm.CurrentSection.IsTypeSizeEditing = false;
    }
  }
}