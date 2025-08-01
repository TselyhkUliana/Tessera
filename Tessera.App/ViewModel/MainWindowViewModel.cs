using MappingManager.ViewModel.Base;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Tessera.App.Command;
using Tessera.App.Model;
using Tessera.DataAccess;

namespace Tessera.App.ViewModel
{
  class MainWindowViewModel : ViewModelBase, ISuggestionProvider
  {
    private const string MATERIAL_TAG = "Material";
    private const string SORTAMENT_TAG = "Sortament";
    private const string SORTAMENT_EX_TAG = "SortamentEx";
    private SectionDefinitionViewModel _сurrentSection;
    private EmbeddingService _embeddingService;
    private string _selectedMaterial;
    private string _selectedProfile;
    private string _selectedInstance;

    public MainWindowViewModel()
    {
      _ = InitiInitializeAsync();
    }

    public IEnumerable<ICommand> Commands => Initializer.Instance.GetCommands(SectionDefinitions, this);

    public ObservableCollection<SectionDefinitionViewModel> SectionDefinitions { get; set; }
    public SectionDefinitionViewModel CurrentSection
    {
      get => _сurrentSection;
      set => Set(ref _сurrentSection, value);
    }
    public ObservableCollection<string> SuggestedMaterials { get; private set; }
    public ObservableCollection<string> SuggestedProfiles { get; private set; }
    public ObservableCollection<string> SuggestedInstances { get; private set; }
    public List<(float[] Id, string Name)> MaterialEmbeddings { get; private set; }
    public List<(float[] Id, string Name)> ProfileEmbeddings { get; private set; }
    public List<(float[] Id, string Name)> InstanceEmbeddings { get; private set; }
    //public string SelectedMaterial
    //{
    //  get => _selectedMaterial;
    //  set
    //  {
    //    if (Set(ref _selectedMaterial, value))
    //      CurrentSection.Material = value;
    //  }
    //}
    //public string SelectedProfile
    //{
    //  get => _selectedProfile;
    //  set
    //  {
    //    if (Set(ref _selectedProfile, value))
    //      CurrentSection.SectionProfile = value;
    //  }
    //}
    //public string SelectedInstance
    //{
    //  get => _selectedInstance;
    //  set
    //  {
    //    if (Set(ref _selectedInstance, value))
    //      CurrentSection.SectionInstance = value;
    //  }
    //}

    public async Task UpdateSuggestionsAsync(string userInput, ObservableCollection<string> suggestionsTarget, List<(float[] Id, string Name)> embeddingDatabase)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();

      float[] embedding = null;
      await Task.Run(() => _embeddingService.GetTextEmbedding(userInput, out embedding));
      var searchResults = await Task.Run(() => _embeddingService.Search(embeddingDatabase, embedding, 10));
      Application.Current.Dispatcher.Invoke(() =>
      {
        suggestionsTarget.Clear();
        foreach (var (Name, Similarity) in searchResults)
          suggestionsTarget.Add(Name);
      });

      stopwatch.Stop();
      Debug.WriteLine($"Search took {stopwatch.ElapsedMilliseconds} - {stopwatch.Elapsed.Seconds} ms");
    }

    private async Task InitiInitializeAsync()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();

      var embeddingService = Task.Run(() => _embeddingService = EmbeddingService.Instance);
      SectionDefinitions = new ObservableCollection<SectionDefinitionViewModel> { new SectionDefinitionViewModel(new SectionDefinition(), this) };
      var class1 = await Class1.CreateAsync();
      MaterialEmbeddings = new List<(float[] Vectors, string Name)>();
      ProfileEmbeddings = new List<(float[] Vectors, string Name)>();
      InstanceEmbeddings = new List<(float[] Vectors, string Name)>();
      await foreach (var (Vectors, Name, UniqueId) in class1.GetElementsWithVectorsAndNamesAsync())
      {
        if (UniqueId.StartsWith(MATERIAL_TAG))
          MaterialEmbeddings.Add((Vectors, Name));
        else if (UniqueId.StartsWith(SORTAMENT_EX_TAG))
          InstanceEmbeddings.Add((Vectors, Name));
        else if (UniqueId.StartsWith(SORTAMENT_TAG))
          ProfileEmbeddings.Add((Vectors, Name));
      }
      await class1.DisposeAsync();
      SuggestedMaterials = new(MaterialEmbeddings.Select(x => x.Name).Take(10).ToArray());
      SuggestedProfiles = new(ProfileEmbeddings.Select(x => x.Name).Take(10).ToArray());
      SuggestedInstances = new(InstanceEmbeddings.Select(x => x.Name).Take(10).ToArray());
      await embeddingService;

      stopwatch.Stop();
      Debug.WriteLine($"MaterialsBD {new string('*', 110)}");
      Debug.WriteLine($"InitiInitializeAsync took {stopwatch.ElapsedMilliseconds} - {stopwatch.Elapsed.Seconds} ms.");
    }
  }
}