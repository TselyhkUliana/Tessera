using MappingManager.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tessera.App.Command;
using Tessera.PolinomProvider.Interface;
using Tessera.PolinomProvider.Model;

namespace Tessera.App.ViewModel
{
  class MainWindowViewModel : ViewModelBase, ISuggestionProvider
  {
    private readonly IReferenceProvider _referenceProvider;
    private readonly CommandManager _commandManager;
    private SectionDefinitionViewModel _сurrentSection;
    private EmbeddingService _embeddingService;
    private bool _isBusy;

    public MainWindowViewModel(IReferenceProvider referenceProvider)
    {
      _referenceProvider = referenceProvider;
      _commandManager = new CommandManager(this, _referenceProvider);
    }

    public ObservableCollection<SectionDefinitionViewModel> SectionDefinitions { get; set; }
    public SectionDefinitionViewModel CurrentSection { get => _сurrentSection; set => Set(ref _сurrentSection, value); }
    public List<(float[] Id, string Name)> MaterialEmbeddings { get; private set; }
    public List<(float[] Id, string Name)> SortamentEmbeddings { get; private set; }

    public ICommand CreateCommand => _commandManager.Get<CheckAndCreateEntitiesCommand>();
    public ICommand AddFileForMaterialCommand => _commandManager.Get<AddFileForMaterialCommand>();
    public ICommand AddFileForSortamentCommand => _commandManager.Get<AddFileForSortamentCommand>();

    public bool IsBusy { get => _isBusy; set => Set((v) => _isBusy = v, _isBusy, value); }

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

    internal async Task InitializeAsync()
    {
      IsBusy = true;
      try
      {
        await InitiInitializeAsync();
      }
      finally
      {
        IsBusy = false;
      }
    }

    private async Task InitiInitializeAsync()
    {
      var embeddingService = await Task.Run(() => _embeddingService = EmbeddingService.Instance);
      MaterialEmbeddings = new List<(float[] Vectors, string Name)>();
      SortamentEmbeddings = new List<(float[] Vectors, string Name)>();
      var elements = await _referenceProvider.LoadElementsWithEmbeddingAsync();
      var splitTask = Task.Run(() =>
      {
        var materials = elements.Items.Where(e => e.Type == "Material").Select(e => (e.EmbeddingVector, e.Name)).ToList();
        var sortaments = elements.Items.Where(e => e.Type == "Sortament").Select(e => (e.EmbeddingVector, e.Name)).ToList();
        return (materials, sortaments);
      });
      var (materialsList, sortamentsList) = await splitTask;
      MaterialEmbeddings = materialsList;
      SortamentEmbeddings = sortamentsList;
      SectionDefinitions = new ObservableCollection<SectionDefinitionViewModel> { NewSectionDefinition() };
      OnPropertyChanged(nameof(SectionDefinitions));
    }

    private void AddSectionDefinitionIfNeeded(object sender, EventArgs e)
    {
      if (SectionDefinitions.Count == 1)
      {
        SectionDefinitions.Add(NewSectionDefinition());
        return;
      }

      var lastElement = SectionDefinitions.LastOrDefault();
      if (lastElement.Material is null && lastElement.Sortament is null && lastElement.TypeSizeViewModel.TypeSize is null && lastElement.SortamentEx is null)
        return;

      SectionDefinitions.Add(NewSectionDefinition());
    }

    private SectionDefinitionViewModel NewSectionDefinition()
    {
      var sectionDefinitionViewModel = new SectionDefinitionViewModel(new SectionDefinition(), this);
      sectionDefinitionViewModel.SuggestedMaterials = new(MaterialEmbeddings.Select(x => x.Name).Take(10).ToArray());
      sectionDefinitionViewModel.SuggestedSortament = new(SortamentEmbeddings.Select(x => x.Name).Take(10).ToArray());
      sectionDefinitionViewModel.RequestAddSectionDefinition += AddSectionDefinitionIfNeeded;
      return sectionDefinitionViewModel;
    }
  }
}