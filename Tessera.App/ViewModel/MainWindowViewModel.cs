using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.App.Model;
using Tessera.App.ViewModel.Base;

namespace Tessera.App.ViewModel
{
  internal class MainWindowViewModel : ViewModelBase
  {
    private SectionDefinition _selectedSectionDefinition;

    public MainWindowViewModel()
    {
      SectionDefinitions = new ObservableCollection<SectionDefinition>();

      SectionDefinitions.CollectionChanged += SectionDefinitions_CollectionChanged;
    }

    public ObservableCollection<SectionDefinition> SectionDefinitions { get; set; }
    public SectionDefinition SelectedSectionDefinition { get => _selectedSectionDefinition; set => Set(ref _selectedSectionDefinition, value); }

    private void SectionDefinitions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)  //TODO наверное не нужно
    {
      if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
      {
        foreach (var newItem in e.NewItems)
        {
          var item = (SectionDefinition)newItem;
        }
      }
    }
  }
}
