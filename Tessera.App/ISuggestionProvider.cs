using System.Collections.ObjectModel;

namespace Tessera.App
{
  interface ISuggestionProvider
  {
    List<(float[] Id, string Name)> MaterialEmbeddings { get; }
    List<(float[] Id, string Name)> ProfileEmbeddings { get; }
    List<(float[] Id, string Name)> InstanceEmbeddings { get; }
    ObservableCollection<string> SuggestedMaterials { get; }
    ObservableCollection<string> SuggestedProfiles { get; }
    ObservableCollection<string> SuggestedInstances { get; }

    Task UpdateSuggestionsAsync(string userInput, ObservableCollection<string> suggestionsTarget, List<(float[] Id, string Name)> embeddingDatabase);
  }
}
