using System.Collections.ObjectModel;

namespace Tessera.App
{
  public interface ISuggestionProvider
  {
    List<(float[] Embedding, string Name)> MaterialEmbeddings { get; }
    List<(float[] Embedding, string Name)> SortamentEmbeddings { get; }

    Task UpdateSuggestionsAsync(string userInput, ObservableCollection<string> suggestionsTarget, List<(float[] Embedding, string Name)> embeddingDatabase);
  }
}
