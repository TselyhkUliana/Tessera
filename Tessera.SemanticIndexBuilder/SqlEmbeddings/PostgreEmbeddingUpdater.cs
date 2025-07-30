using Tessera.DataAccess;

namespace Tessera.SemanticIndexBuilder.SqlEmbeddings
{
  internal class PostgreEmbeddingUpdater : IElementEmbeddingUpdater
  {
    public async void UpdateEmbeddings(string DatabaseName)
    {
      var @class = await Class1.CreateAsync();
      var (isException, message) = @class.EnsureVectorColumnExists();
      if (isException && !string.IsNullOrEmpty(message))
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        return;
      }
      if (!isException && !string.IsNullOrEmpty(message))
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
      }

      var idsAndNames = @class.GetElementIdsWithNames().ToList();
      if (idsAndNames.Count == 0)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Нет элементов для обновления векторов.");
        Console.ResetColor();
        return;
      }

      Console.ForegroundColor = ConsoleColor.Blue;
      Console.WriteLine("Создание и обновление векторных предствлений. Процесс может занять длительное время");
      Console.ForegroundColor = ConsoleColor.Green;
      var embeddingService = EmbeddingService.Instance;
      var count = idsAndNames.Count;
      for (int i = 0; i < count; i++)
      {
        embeddingService.GetTextEmbedding(idsAndNames[i].Name, out var embedding);
        @class.UpdateElementVector(embedding, idsAndNames[i].Id);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"\rОбновлено: {i + 1} из {count}");
      }
      embeddingService.Dispose();
      await @class.DisposeAsync();

      Console.ForegroundColor = ConsoleColor.Blue;
      Console.WriteLine("\nОбновление векторов завершено.");
      Console.WriteLine("Нажмите любую клавишу");
      Console.ReadKey();
    }
  }
}
