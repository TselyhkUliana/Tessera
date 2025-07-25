using Ascon.Polynom.Api;
using Ascon.Polynom.Login;
using Npgsql;
using System.Threading.Tasks;
using Tessera.SemanticIndexBuilder;

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
        break;
      case DbType.PostgreSql:
        new Postgre().Start(storage.Database);
        break;
      case DbType.Oracle:
        break;
      default:
        break;
    }
  }

  enum DbType
  {
    SqlServer,
    PostgreSql,
    Oracle
  }
}