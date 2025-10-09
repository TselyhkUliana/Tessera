using System.Collections.ObjectModel;

namespace Tessera.App
{
  public interface ISuggestionProvider
  {
    List<(float[] embedding, string name)> MaterialEmbeddings { get; }
    List<(float[] embedding, string name)> SortamentEmbeddings { get; }

    Task UpdateSuggestionsAsync(string userInput, ObservableCollection<string> suggestionsTarget, List<(float[] embedding, string name)> embeddingDatabase);
  }
}
