using Ascon.Polynom.Api;
using Ascon.Polynom.Login;
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