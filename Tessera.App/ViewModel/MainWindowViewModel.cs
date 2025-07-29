using MappingManager.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Tessera.App.Model;
using Tessera.DataAccess;

namespace Tessera.App.ViewModel
{
  class MainWindowViewModel : ViewModelBase
  {
    private SectionDefinition _selectedSectionDefinition;
    private EmbeddingService _embeddingService;

    public MainWindowViewModel()
    {
      SectionDefinitions = new();
      _ = InitiInitializeAsync();
      //_embeddingService = EmbeddingService.Instance;
    }

    public ObservableCollection<SectionDefinition> SectionDefinitions { get; set; }
    public SectionDefinition SelectedSectionDefinition { get => _selectedSectionDefinition; set => Set(ref _selectedSectionDefinition, value); }
    public List<(float[] Id, string Name)> Materials { get; private set; }
    public List<(float[] Id, string Name)> SectionProfiles { get; private set; }
    public List<(float[] Id, string Name)> SectionInstance { get; private set; }

    public async Task InitiInitializeAsync()
    { //TODO:подумать над параллельной загрузкой данныхдля ускорения сейчас примерно 10 секунд
      Stopwatch stopwatch = Stopwatch.StartNew();
      var class1 = Class1.Instance;
      Materials = new List<(float[] Vectors, string Name)>();
      SectionProfiles = new List<(float[] Vectors, string Name)>();
      SectionInstance = new List<(float[] Vectors, string Name)>();
      await foreach (var (Vectors, Name, UniqueId) in class1.GetElementsWithVectorsAndNamesAsync())
      {
        if (UniqueId.StartsWith("Material"))
          Materials.Add((Vectors, Name));
        else if (UniqueId.StartsWith("SortamentEx"))
          SectionInstance.Add((Vectors, Name));
        else if (UniqueId.StartsWith("Sortament"))
          SectionProfiles.Add((Vectors, Name));
      }
      class1.Dispose();
      stopwatch.Stop();
      Debug.WriteLine($"Initialization took {stopwatch.ElapsedMilliseconds} - {stopwatch.Elapsed.Seconds} ms");
      //OnPropertyChanged(nameof(Materials));
      //TypeSizes = new List<(float[] Vectors, string Name)>();
      //Materials = new ObservableCollection<(float[] Id, string Name)>(tempMaterials);
      //SectionProfiles = new ObservableCollection<(float[] Id, string Name)>(tempSectionProfiles);
      //TypeSizes = new ObservableCollection<(float[] Id, string Name)>(tempTypeSizes);
      //SectionInstance = new ObservableCollection<(float[] Id, string Name)>(tempSectionInstance);
      //Others = new ObservableCollection<(float[] Id, string Name)>(tempOthers);

      //var elements = await Task.Run(() => class1.GetElementsWithVectorsAndNames().ToList());
      //Materials = new ObservableCollection<string>(elements.Where(x => x.UniqueId.StartsWith("Material")).Select(x => x.Name).Take(200).ToList());
    }
    //public List<(float[] Id, string Name)> TypeSizes { get; set; }
    //public List<(float[] Id, string Name)> Others { get; set; }
  }
}
