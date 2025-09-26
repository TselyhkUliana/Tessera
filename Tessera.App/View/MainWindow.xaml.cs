using System.Windows;
using System.Windows.Controls;
using Tessera.App.ViewModel;
using Tessera.PolinomProvider;

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
        vm.CurrentSection.TypeSizeViewModel.IsTypeSizeEditing = e.Column.Header.ToString() == "Типоразмеры";
    }

    private void MyGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
      //подумать что сделать чтобы после потери фокуса с ячейки редактирования не сбрасывался IsTypeSizeEditing
      //if (DataContext is MainWindowViewModel vm)
      //  vm.CurrentSection.TypeSizeViewModel.IsTypeSizeEditing = false;
    }
  }
}