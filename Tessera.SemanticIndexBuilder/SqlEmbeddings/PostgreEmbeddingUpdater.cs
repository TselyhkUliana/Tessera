using Ascon.Polynom.Api;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.SemanticIndexBuilder.SqlEmbeddings
{
  internal class PostgreEmbeddingUpdater : IElementEmbeddingUpdater
  {
    public void UpdateEmbeddings(string DatabaseName)
    {
      var connectionString = $"Host=localhost;Port=5432;Username=postgres;Password=111;Database={DatabaseName}"; //TODO: найти пароль
      using var connection = new NpgsqlConnection(connectionString);
      connection.Open();
      try
      {
        var addColumnCmd = new NpgsqlCommand("ALTER TABLE \"POLYNOM\".\"ELEMENTDATA\" ADD COLUMN \"VECTOR\" float8[];", connection);
        addColumnCmd.ExecuteNonQuery();
      }
      catch (PostgresException ex) when (ex.SqlState == "42701") // duplicate_column
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Колонка 'VECTOR' уже существует в таблице 'ELEMENTDATA'. Пропускаем добавление.");
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        return;
      }

      var selectCmd = new NpgsqlCommand("SELECT \"OID\", \"NAME\" FROM \"POLYNOM\".\"ELEMENTDATA\" WHERE \"VECTOR\" IS NULL;", connection);
      var idsAndNames = new List<(int Id, string Name)>();
      using (var reader = selectCmd.ExecuteReader())
      {
        while (reader.Read())
        {
          var id = reader.GetInt32(0);
          string name = reader.GetString(1);
          idsAndNames.Add((id, name));
        }
      }

      if(idsAndNames.Count == 0)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Нет элементов для обновления векторов.");
        Console.ResetColor();
        return;
      }

      Console.ForegroundColor = ConsoleColor.Blue;
      Console.WriteLine("Создание и обновление вектрных предствлений. Процесс может занять длительное время");
      Console.ForegroundColor = ConsoleColor.Green;
      var embeddingService  = EmbeddingService.Instance;
      var count = idsAndNames.Count;
      for (int i = 0; i < count; i++)
      {
        using var updateCmd = new NpgsqlCommand("UPDATE \"POLYNOM\".\"ELEMENTDATA\" SET \"VECTOR\" = @vec WHERE \"OID\" = @id", connection);

        embeddingService.GetTextEmbedding(idsAndNames[i].Name, out var embedding);
        updateCmd.Parameters.AddWithValue("vec", embedding);
        updateCmd.Parameters.AddWithValue("id", idsAndNames[i].Id);

        updateCmd.ExecuteNonQuery();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"\rОбновлено: {i + 1} из {count}");
      }
      embeddingService.Dispose();
      
      Console.ForegroundColor = ConsoleColor.Blue;
      Console.WriteLine("\nОбновление векторов завершено.");
      Console.WriteLine("Нажмите любую клавишу");
      Console.ReadKey();
    }
  }
}
