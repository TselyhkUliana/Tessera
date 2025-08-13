using System.Collections.ObjectModel;

namespace Tessera.App
{
  public interface ISuggestionProvider
  {
    List<(float[] Id, string Name)> MaterialEmbeddings { get; }
    List<(float[] Id, string Name)> ProfileEmbeddings { get; }
    List<(float[] Id, string Name)> InstanceEmbeddings { get; }

    Task UpdateSuggestionsAsync(string userInput, ObservableCollection<string> suggestionsTarget, List<(float[] Id, string Name)> embeddingDatabase);
  }
}
