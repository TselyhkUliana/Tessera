using Tessera.PolinomProvider.Interface;

namespace Tessera.SemanticIndexBuilder.Embeddings
{
  internal class PolinomEmbeddingUpdater : IElementEmbeddingUpdater
  {
    public async Task UpdateEmbeddings(IEmbeddingProvider embeddingProvider)
    {
      if (embeddingProvider.EnsureVectorConceptExists())
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Создано понятие 'Семантическое представление'");
      }
      Console.ForegroundColor = ConsoleColor.Blue;
      Console.WriteLine("Создание векторных предствлений. Процесс может занять длительное время");
      Console.ForegroundColor = ConsoleColor.Green;
      var embeddingService = EmbeddingService.Instance;
      var elements = (await embeddingProvider.LoadElementsForEmbeddingAsync()).ToArray();
      var count = elements.Length;
      for (int i = 0; i < count; i++)
      {
        var element = elements[i];
        embeddingService.GetTextEmbedding(element.Name, out var embedding);
        embeddingProvider.CreateElementEmbedding(element.Location, string.Join(';', embedding));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"\rСоздано: {i + 1} из {count}");
      }
      embeddingService.Dispose();
      Console.ForegroundColor = ConsoleColor.Blue;
      Console.WriteLine("\nСоздание векторов завершено.");
      Console.WriteLine("Нажмите любую клавишу");
      Console.ReadKey();
    }
  }
}
