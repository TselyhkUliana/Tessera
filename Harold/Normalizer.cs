using System.Text.RegularExpressions;

public static class Normalizer
{
  private static readonly Dictionary<string, string> substitutions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "зл", "золото" },
        { "ц0", "цинк" },
        { "амф", "алюминий" },
        { "мд", "медь" },
        { "нпан", "непрерывное анодирование" },
        { "ан", "анод" },
        { "гпрхх", "графит" },
        { "говнх", "графит" } // даже если это опечатка :)
    };

  public static string Normalize(string input)
  {
    string result = input.ToLowerInvariant();

    // Заменяем сокращения
    foreach (var kvp in substitutions)
    {
      result = Regex.Replace(result, $@"\b{Regex.Escape(kvp.Key)}\b", kvp.Value, RegexOptions.IgnoreCase);
    }

    // Удаляем лишние знаки препинания и лишние пробелы
    result = Regex.Replace(result, @"[^\w\d\s]", " ");
    result = Regex.Replace(result, @"\s+", " ").Trim();

    return result;
  }
}
