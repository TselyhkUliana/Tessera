using MappingManager.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Tessera.App.Command;
using Tessera.PolinomProvider;
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
    private bool _isBusy = true;

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

    public bool IsBusy { get => _isBusy; set => Set((v) => _isBusy = v, _isBusy, value); }

    public async Task UpdateSuggestionsAsync(string userInput, ObservableCollection<string> suggestionsTarget, List<(float[] Id, string Name)> embeddingDatabase)
    {
      var searchResults = await Task.Run(() =>
      {
        _embeddingService.GetTextEmbedding(userInput, out var embedding);
        return _embeddingService.Search(embeddingDatabase, embedding, userInput);
      });

      suggestionsTarget.Clear();
      foreach (var (Name, Similarity) in searchResults)
        suggestionsTarget.Add(Name);
    }

    internal async Task InitializeAsync()
    {
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
      var embeddingServiceTask = Task.Run(() => EmbeddingService.Instance);
      MaterialEmbeddings = new List<(float[] Vectors, string Name)>();
      SortamentEmbeddings = new List<(float[] Vectors, string Name)>();
      var elements = await _referenceProvider.LoadElementsWithEmbeddingAsync();
      var materialsTask = Task.Run(() => elements.Items.Where(e => e.Type == ElementType.Material.ToString()).Select(e => (e.EmbeddingVector, e.Name)).ToList());
      var sortamentsTask = Task.Run(() => elements.Items.Where(e => e.Type == ElementType.Sortament.ToString()).Select(e => (e.EmbeddingVector, e.Name)).ToList());
      await Task.WhenAll(materialsTask, sortamentsTask, embeddingServiceTask);
      MaterialEmbeddings = materialsTask.Result;
      SortamentEmbeddings = sortamentsTask.Result;
      SectionDefinitions = new ObservableCollection<SectionDefinitionViewModel> { NewSectionDefinition() };
      OnPropertyChanged(nameof(SectionDefinitions));
      _embeddingService = embeddingServiceTask.Result;
    }

    private void AddSectionDefinitionIfNeeded(object sender, EventArgs e)
    {
      if (SectionDefinitions.Count == 1)
      {
        SectionDefinitions.Add(NewSectionDefinition());
        return;
      }
      var penultimate = SectionDefinitions[^2];
      if (IsSectionDefinitionEmpty(penultimate))
      {
        SectionDefinitions.Remove(penultimate);
        SectionDefinitions.Add(NewSectionDefinition());
        return;
      }

      if (IsSectionDefinitionEmpty(SectionDefinitions.LastOrDefault()))
        return;

      SectionDefinitions.Add(NewSectionDefinition());
    }

    private static bool IsSectionDefinitionEmpty(SectionDefinitionViewModel sectionDefinition)
    {
      return string.IsNullOrEmpty(sectionDefinition.Material) && string.IsNullOrEmpty(sectionDefinition.Sortament) && string.IsNullOrEmpty(sectionDefinition.TypeSizeViewModel.TypeSize);
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