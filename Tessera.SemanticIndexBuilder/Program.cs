using Ascon.Polynom.Api;
using Ascon.Polynom.Login;
using Npgsql;
using System.Diagnostics;
using Tessera.SemanticIndexBuilder;
using Tessera.SemanticIndexBuilder.SqlEmbeddings;

internal class Program
{
  [STAThread]
  private static void Main(string[] args)
  {
    if (!LoginManager.TryOpenSession(Guid.Empty, SessionOptions.None, ClientType.Client, out var session))
      return;

    var storage = session.Storage;
    Enum.TryParse<DbType>(storage.DbmsType, out var db);
    switch (db)
    {
      case DbType.SqlServer:
        //UpdateElementEmbeddings(storage.Database, new MSServerEmbeddingUpdater());
        break;
      case DbType.PostgreSql:
        UpdateElementEmbeddings(storage.Database, new PostgreEmbeddingUpdater());
        #region Test
        //var connectionString = $"Host=localhost;Port=5432;Username=postgres;Password=111;Database={storage.Database}";
        //using (var connection = new NpgsqlConnection(connectionString))
        //{
        //  connection.Open();
        //  var embeddingService = new EmbeddingService();
        //  embeddingService.GetTextEmbedding("Анод ДПРХХ 5х140х460 М1", out var embedding);
        //  var selectCmd = new NpgsqlCommand("SELECT \"VECTOR\", \"NAME\" FROM \"POLYNOM\".\"ELEMENTDATA\";", connection);
        //  Stopwatch sw = new Stopwatch();
        //  sw.Start();
        //  var idsAndNames = new List<(float[] Id, string Name)>();
        //  using (var reader = selectCmd.ExecuteReader())
        //  {
        //    while (reader.Read())
        //    {
        //      var id = (double[])reader[0];
        //      var ids = id.Select(x => (float)x).ToArray();
        //      string name = reader.GetString(1);
        //      idsAndNames.Add((ids, name));
        //    }
        //  }
        //  sw.Stop();
        //  Console.WriteLine($"Получено {idsAndNames.Count} векторов за {sw.Elapsed.TotalSeconds} секунд");

        //  Stopwatch sw2 = new Stopwatch();
        //  sw2.Start();
        //  var res = embeddingService.Search(idsAndNames, embedding, 5);
        //  sw2.Stop();
        //  Console.WriteLine(sw2.Elapsed.TotalSeconds);
        //  Console.ReadKey();
        //}
        #endregion
        break;
      default:
        break;
    }
  }

  private static void UpdateElementEmbeddings(string database, IElementEmbeddingUpdater updater)
  {
    var manager = new ElementEmbeddingUpdateManager(database, updater);
    manager.Update();
  }

  enum DbType
  {
    SqlServer,
    PostgreSql
  }
}