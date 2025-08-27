using MappingManager.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
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
    private CommandManager _commandManager;

    public MainWindowViewModel()
    {
      _ = InitiInitializeAsync();
      _commandManager = new CommandManager(this);
    }

    public ObservableCollection<SectionDefinitionViewModel> SectionDefinitions { get; set; }
    public SectionDefinitionViewModel CurrentSection { get => _сurrentSection; set => Set(ref _сurrentSection, value); }
    public List<(float[] Id, string Name)> MaterialEmbeddings { get; private set; }
    public List<(float[] Id, string Name)> ProfileEmbeddings { get; private set; }
    public List<(float[] Id, string Name)> InstanceEmbeddings { get; private set; }

    public ICommand CreateCommand => _commandManager.Get<CheckAndCreateEntitiesCommand>();
    public ICommand AddFileForMaterialCommand => _commandManager.Get<AddFileForMaterialCommand>();
    public ICommand AddFileForSortamentCommand => _commandManager.Get<AddFileForSortamentCommand>();

    public async Task UpdateSuggestionsAsync(string userInput, ObservableCollection<string> suggestionsTarget, List<(float[] Id, string Name)> embeddingDatabase)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();

      var searchResults = await Task.Run(() =>
      {
        _embeddingService.GetTextEmbedding(userInput, out var embedding);
        return _embeddingService.Search(embeddingDatabase, embedding, 10);
      });

      suggestionsTarget.Clear();
      foreach (var (Name, Similarity) in searchResults)
        suggestionsTarget.Add(Name);

      stopwatch.Stop();
      Debug.WriteLine($"Search took {stopwatch.ElapsedMilliseconds} - {stopwatch.Elapsed.Seconds} ms");
    }

    private async Task InitiInitializeAsync()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();

      var embeddingService = Task.Run(() => _embeddingService = EmbeddingService.Instance);
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
      SectionDefinitions = new ObservableCollection<SectionDefinitionViewModel> { NewSectionDefinition() };
      OnPropertyChanged(nameof(SectionDefinitions));
      await class1.DisposeAsync();
      await embeddingService;

      stopwatch.Stop();
      Debug.WriteLine($"MaterialsBD {new string('*', 110)}");
      Debug.WriteLine($"InitiInitializeAsync took {stopwatch.ElapsedMilliseconds} - {stopwatch.Elapsed.Seconds} ms.");
    }

    private void AddSectionDefinitionIfNeeded()
    {
      if (SectionDefinitions.Count == 1)
      {
        SectionDefinitions.Add(NewSectionDefinition());
        return;
      }

      var lastElement = SectionDefinitions.LastOrDefault();
      if (lastElement.Material is null && lastElement.SectionProfile is null && lastElement.SectionInstance is null)
        return;

      SectionDefinitions.Add(NewSectionDefinition());
    }

    private SectionDefinitionViewModel NewSectionDefinition()
    {
      var sectionDefinitionViewModel = new SectionDefinitionViewModel(new SectionDefinition(), this);
      sectionDefinitionViewModel.SuggestedMaterials = new(MaterialEmbeddings.Select(x => x.Name).Take(10).ToArray());
      sectionDefinitionViewModel.SuggestedProfiles = new(ProfileEmbeddings.Select(x => x.Name).Take(10).ToArray());
      sectionDefinitionViewModel.SuggestedInstances = new(InstanceEmbeddings.Select(x => x.Name).Take(10).ToArray());
      sectionDefinitionViewModel.RequestAddSectionDefinition += AddSectionDefinitionIfNeeded;
      return sectionDefinitionViewModel;
    }
  }
}