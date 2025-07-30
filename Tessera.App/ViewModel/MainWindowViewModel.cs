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
    private SectionDefinition _сurrentSection;
    private EmbeddingService _embeddingService;
    private string _materialInput;
    private string _profileInput;
    private string _instanceInput;

    public MainWindowViewModel()
    {
      SectionDefinitions = new();
      _ = InitiInitializeAsync();
      _embeddingService = EmbeddingService.Instance;
    }

    public ObservableCollection<SectionDefinition> SectionDefinitions { get; set; }
    public SectionDefinition CurrentSection
    {
      get => _сurrentSection;
      set => Set(ref _сurrentSection, value);
    }
    public ObservableCollection<string> SuggestedMaterials { get; set; }
    public ObservableCollection<string> SuggestedProfiles { get; set; }
    public ObservableCollection<string> SuggestedInstances { get; set; }
    public string MaterialInput
    {
      get => _materialInput;
      set
      {
        if (!Set(ref _materialInput, value))
          return;

        _ = UpdateSuggestions(value, SuggestedMaterials, MaterialEmbeddings);
        CurrentSection.Material = value;
      }
    }
    public string ProfileInput
    {
      get => _profileInput;
      set
      {
        if (!Set(ref _profileInput, value))
          return;

        _ = UpdateSuggestions(value, SuggestedProfiles, ProfileEmbeddings);
        CurrentSection.SectionProfile = value;
      }
    }
    public string InstanceInput
    {
      get => _instanceInput;
      set
      {
        if (!Set(ref _instanceInput, value))
          return;

        _ = UpdateSuggestions(value, SuggestedInstances, InstanceEmbeddings);
        CurrentSection.SectionInstance = value;
      }
    }
    public List<(float[] Id, string Name)> MaterialEmbeddings { get; private set; }
    public List<(float[] Id, string Name)> ProfileEmbeddings { get; private set; }
    public List<(float[] Id, string Name)> InstanceEmbeddings { get; private set; }

    private async Task UpdateSuggestions(string userInput, ObservableCollection<string> suggestionsTarget, List<(float[] Id, string Name)> embeddingDatabase)
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

    public async Task InitiInitializeAsync()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      var class1 = await Class1.CreateAsync();
      MaterialEmbeddings = new List<(float[] Vectors, string Name)>();
      ProfileEmbeddings = new List<(float[] Vectors, string Name)>();
      InstanceEmbeddings = new List<(float[] Vectors, string Name)>();
      await foreach (var (Vectors, Name, UniqueId) in class1.GetElementsWithVectorsAndNamesAsync())
      {
        if (UniqueId.StartsWith("Material"))
          MaterialEmbeddings.Add((Vectors, Name));
        else if (UniqueId.StartsWith("SortamentEx"))
          InstanceEmbeddings.Add((Vectors, Name));
        else if (UniqueId.StartsWith("Sortament"))
          ProfileEmbeddings.Add((Vectors, Name));
      }
      await class1.DisposeAsync();
      SuggestedMaterials = new(MaterialEmbeddings.Select(x => x.Name).Take(10).ToArray());
      SuggestedProfiles = new(ProfileEmbeddings.Select(x => x.Name).Take(10).ToArray());
      SuggestedInstances = new(InstanceEmbeddings.Select(x => x.Name).Take(10).ToArray());
      stopwatch.Stop();
      Debug.WriteLine($"MaterialsBD {new string('*', 110)}");
      Debug.WriteLine($"InitiInitializeAsync took {stopwatch.ElapsedMilliseconds} - {stopwatch.Elapsed.Seconds} ms.");
    }
  }
}
