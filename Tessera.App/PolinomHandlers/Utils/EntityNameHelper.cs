namespace Tessera.App.PolinomHandlers.Utils
{
  internal static class EntityNameHelper
  {
    /// <summary>
    /// Возвращает часть имени, расположенную перед первым найденным в строке упоминанием любого стандарта из <see cref="Constants.Standards"/> 
    /// <br/>Если ни один стандарт не найден, возвращает обрезанное (Trim) исходное имя
    /// </summary>
    public static string GetNameBeforeStandard(string fullName)
    {
      int index = GetStandardKeywordIndex(fullName);
      return index > 0 ? fullName.Substring(0, index).Trim() : fullName.Trim();
    }

    /// <summary>
    /// Находит индекс первого упоминания любого стандарта из <see cref="Constants.Standards"/> в имени (без учёта регистра). 
    /// <br/>Если ни один стандарт не найден, возвращает -1
    /// </summary>
    public static int GetStandardKeywordIndex(string fullName)
    {
      return Constants.Standards
         .Select(x => fullName.IndexOf(x, StringComparison.OrdinalIgnoreCase))
         .Where(index => index >= 0)
         .DefaultIfEmpty(-1)
         .First();
    }

    /// <summary>
    /// Убирает стандарт, часть после закрывающей скобки, лишние пробелы, скобки и запятые.
    /// </summary>
    public static string FormatNameParts(string input)
    {
      var baseName = GetNameBeforeStandard(input);

      var closingParenIndex = baseName.IndexOf(')');
      if (closingParenIndex > -1)
        baseName = baseName.Substring(0, closingParenIndex);

      var parts = baseName.Split(new[] { ' ', '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

      return string.Join(" ", parts);
    }

    /// <summary>
    /// Возвращает часть строки начиная с ключевого слова стандарта 
    /// <br/>(например: "ГОСТ 10003-90").
    /// </summary>
    public static string GetFullStandard(string fullName)
    {
      if (string.IsNullOrWhiteSpace(fullName))
        return string.Empty;

      var index = GetStandardKeywordIndex(fullName);
      return index > 0 ? fullName.Substring(index).Trim() : string.Empty;
    }

    /// <summary>
    /// Возвращает ключевое слово стандарта (например: "ГОСТ", "ISO"), 
    /// <br/> если строка начинается с одного из стандартов в <see cref="Constants.Standards"/>.
    /// </summary>
    public static string GetStandard(string fullStandard)
    {
      if (string.IsNullOrWhiteSpace(fullStandard))
        return string.Empty;

      return Constants.Standards.FirstOrDefault(x => fullStandard.StartsWith(x, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }

    /// <summary>
    /// Приводит полное имя к формату: первая буква — заглавная, стандарт — в верхнем регистре.
    /// </summary>
    public static string FormatFullName(string fullName)
    {
      if (string.IsNullOrWhiteSpace(fullName))
        return string.Empty;

      var normalized = fullName.FirstCharToUpper();
      var standard = GetFullStandard(normalized);

      if (!string.IsNullOrEmpty(standard))
      {
        var index = GetStandardKeywordIndex(normalized);
        if (index > 0)
          return normalized.Substring(0, index).Trim() + " " + standard.ToUpper();
      }
      return normalized;
    }
  }
}