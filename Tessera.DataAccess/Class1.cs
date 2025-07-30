using Npgsql;
using System.Data;
using System.Diagnostics;

namespace Tessera.DataAccess
{
  public class Class1 : IAsyncDisposable
  {
    private NpgsqlConnection _connection;
    private bool _disposed;

    public static async Task<Class1> CreateAsync()
    {
      var instance = new Class1();
      var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=111;Database=HaroldBase";
      instance._connection = new NpgsqlConnection(connectionString);
      await instance._connection.OpenAsync();
      return instance;
    }

    /// <summary>
    /// Возвращает все элементы из таблицы ELEMENTDATA, у которых уже заполнена колонка VECTOR.
    /// <br/> Каждая запись содержит массив векторов и имя.
    /// </summary>
    public async IAsyncEnumerable<(float[] Vectors, string Name, string UniqueId)> GetElementsWithVectorsAndNamesAsync()
    {
      var selectCmd = new NpgsqlCommand("SELECT \"NAME\", \"VECTOR\", \"UNIQUEID\" FROM \"POLYNOM\".\"ELEMENTDATA\" WHERE \"VECTOR\" IS NOT NULL AND (\"UNIQUEID\" LIKE 'Material%' OR \"UNIQUEID\" LIKE 'Sortament%' OR \"UNIQUEID\" LIKE 'SortamentEx%');", _connection);
      
      using var reader = await selectCmd.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        var name = reader.GetString(0);

        var vectorsDb = await reader.GetFieldValueAsync<double[]>(1);
        if (vectorsDb == null)
          continue;
        var vectors = new float[vectorsDb.Length];
        for (int i = 0; i < vectorsDb.Length; i++)
          vectors[i] = (float)vectorsDb[i];
        var uniqueId = reader.GetString(2);
        yield return (vectors, name, uniqueId);
      }
    }

    public IEnumerable<(float[] Vectors, string Name, string UniqueId)> GetElementsWithVectorsAndNames()
    {
      var selectCmd = new NpgsqlCommand("SELECT \"NAME\", \"VECTOR\", \"UNIQUEID\" FROM \"POLYNOM\".\"ELEMENTDATA\" WHERE \"VECTOR\" IS NOT NULL;", _connection);
      using var reader = selectCmd.ExecuteReader();
      while (reader.Read())
      {
        var name = reader.GetString(0);
        var vectors = reader.GetFieldValue<double[]>(1);
        var uniqueId = reader.GetString(2);
        yield return (vectors.Select(x => (float)x).ToArray(), name, uniqueId);
      }
    }

    /// <summary>
    /// Возвращает идентификаторы и имена элементов, у которых отсутствует вектор (VECTOR IS NULL).
    /// </summary>
    public IEnumerable<(int Id, string Name)> GetElementIdsWithNames()
    {
      var selectCmd = new NpgsqlCommand("SELECT \"OID\", \"NAME\" FROM \"POLYNOM\".\"ELEMENTDATA\" WHERE \"VECTOR\" IS NULL;", _connection);
      using var reader = selectCmd.ExecuteReader();
      while (reader.Read())
      {
        var id = reader.GetInt32(0);
        var name = reader.GetString(1);
        yield return (id, name);
      }
    }

    /// <summary>
    /// Проверяет наличие колонки VECTOR в таблице ELEMENTDATA. 
    /// <br/> Если колонки нет — добавляет её. 
    /// <br/> Возвращает информацию об успешности или ошибке.
    /// </summary>
    public (bool isException, string message) EnsureVectorColumnExists()
    {
      try
      {
        var addColumnCmd = new NpgsqlCommand("ALTER TABLE \"POLYNOM\".\"ELEMENTDATA\" ADD COLUMN \"VECTOR\" float8[];", _connection);
        addColumnCmd.ExecuteNonQuery();
      }
      catch (PostgresException ex) when (ex.SqlState == "42701") // duplicate_column
      {
        return (false, "Колонка 'VECTOR' уже существует в таблице 'ELEMENTDATA'. Пропускаем добавление.");
      }
      catch (Exception ex)
      {
        return (true, ex.Message);
      }
      return (false, string.Empty);
    }

    /// <summary>
    /// Обновляет значение колонки VECTOR у конкретного элемента по его ID.
    /// <br/> Используется для записи эмбеддингов в базу данных.
    /// </summary>
    public void UpdateElementVector(float[] embedding, int id)
    {
      using var updateCmd = new NpgsqlCommand("UPDATE \"POLYNOM\".\"ELEMENTDATA\" SET \"VECTOR\" = @vec WHERE \"OID\" = @id", _connection);

      updateCmd.Parameters.AddWithValue("vec", embedding);
      updateCmd.Parameters.AddWithValue("id", id);

      updateCmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Освобождает ресурсы, используемые соединением с базой данных.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
      if (!_disposed)
      {
        if (_connection != null)
          await _connection.DisposeAsync();

        _disposed = true;
        GC.SuppressFinalize(this);
      }
    }
  }
}