using Npgsql;

namespace Tessera.DataAccess
{
  public class Class1 : IDisposable
  {
    private static readonly Lazy<Class1> _instance = new Lazy<Class1>(() => new Class1());
    private NpgsqlConnection _connection;
    private bool _disposed;

    private Class1()
    {
      var connectionString = $"Host=localhost;Port=5432;Username=postgres;Password=111;Database=HaroldBase"; //TODO: придумать что делать с паролем и названием бд
      _connection = new NpgsqlConnection(connectionString);
      _connection.Open();
    }

    ~Class1()
    {
      Dispose(false);
    }

    public static Class1 Instance => _instance.Value;

    /// <summary>
    /// Возвращает все элементы из таблицы ELEMENTDATA, у которых уже заполнена колонка VECTOR.
    /// <br/> Каждая запись содержит массив векторов и имя.
    /// </summary>
    public IEnumerable<(float[] Vectors, string Name)> GetElementsWithVectorsAndNames()
    {
      var selectCmd = new NpgsqlCommand("SELECT \"NAME\", \"VECTOR\" FROM \"POLYNOM\".\"ELEMENTDATA\" WHERE \"VECTOR\" IS NOT NULL;", _connection);
      using var reader = selectCmd.ExecuteReader();
      while (reader.Read())
      {
        var name = reader.GetString(0);
        var vectors = reader.GetFieldValue<double[]>(1);
        yield return (vectors.Select(x => (float)x).ToArray(), name);
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

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
          _connection?.Dispose();

        _disposed = true;
      }
    }
  }
}