using Npgsql;

namespace Tessera.DataAccess
{
  public class Class1
  {
    public IEnumerable<(float[] Vectors, string Name)> GetData()
    {
      var connectionString = $"Host=localhost;Port=5432;Username=postgres;Password=111;Database=HaroldBase"; //TODO: найти пароль
      using var connection = new NpgsqlConnection(connectionString);
      connection.Open();

      var selectCmd = new NpgsqlCommand("SELECT \"NAME\", \"VECTOR\" FROM \"POLYNOM\".\"ELEMENTDATA\" WHERE \"VECTOR\" IS NOT NULL;", connection);
      var vertorsAndNames = new List<(float[] Id, string Name)>();
      using var reader = selectCmd.ExecuteReader();
      var vectors = (double[])reader[0];
      var name = reader.GetString(1);
      yield return (vectors.Select(x => (float)x).ToArray(), name);
    }
  }
}