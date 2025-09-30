using MappingManager.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

    public MainWindowViewModel(IReferenceProvider referenceProvider)
    {
      _referenceProvider = referenceProvider;
      _ = InitiInitializeAsync();
      _commandManager = new CommandManager(this, _referenceProvider);
    }

    public ObservableCollection<SectionDefinitionViewModel> SectionDefinitions { get; set; }
    public SectionDefinitionViewModel CurrentSection { get => _сurrentSection; set => Set(ref _сurrentSection, value); }
    public List<(float[] Id, string Name)> MaterialEmbeddings { get; private set; }
    public List<(float[] Id, string Name)> SortamentEmbeddings { get; private set; }

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
      //var test = Serializer.Instance;
      //var poi = test.Items.Where(x => x.Category == "0");
      //foreach (var item in poi)
      //{
      //  Debug.WriteLine($"{item.Name}");
      //}
      var embeddingService = Task.Run(() => _embeddingService = EmbeddingService.Instance);
      MaterialEmbeddings = new List<(float[] Vectors, string Name)>();
      SortamentEmbeddings = new List<(float[] Vectors, string Name)>();
      var elements = await _referenceProvider.LoadElementsWithEmbeddingAsync();
      foreach (var element in elements.Items)
      {
        if (element.Type.Equals("Material"))
          MaterialEmbeddings.Add((element.EmbeddingVector, element.Name));
        else
          SortamentEmbeddings.Add((element.EmbeddingVector, element.Name));
      }
      SectionDefinitions = new ObservableCollection<SectionDefinitionViewModel> { NewSectionDefinition() };
      OnPropertyChanged(nameof(SectionDefinitions));
      await embeddingService;
    }

    //public void Test()
    //{
    //  var serializer = new Serializer();
    //  PropertyList propertyList = new PropertyList();
    //  //List<(string Name, string Embedding)> list = new List<(string Name, string Embedding)>();
    //  foreach (var item in _referenceProvider.Test())
    //  {
    //    _embeddingService.GetTextEmbedding(item, out var embedding);
    //    propertyList.Items.Add(new PropertyItem
    //    {
    //      Name = item,
    //      Embedding = string.Join(";", embedding),
    //    });
    //  }
    //  serializer.MyProperty = propertyList;
    //  serializer.Save();

    //foreach (var item in list)
    //{
    //  Debug.WriteLine($"Name = {item.Name} Embedding = {item.Embedding}");
    //}
    //}

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