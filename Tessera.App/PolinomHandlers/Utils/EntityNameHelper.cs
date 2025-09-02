using Tessera.App.Extensions;
using Tessera.App.PolinomHandlers.Utils.Constants;

namespace Tessera.App.PolinomHandlers.Utils
{
  internal static class EntityNameHelper
  {
    /// <summary>
    /// Возвращает часть имени, расположенную перед первым найденным в строке упоминанием любого стандарта из <see cref="StandardConstants.Standards"/> 
    /// <br/>Если ни один стандарт не найден, возвращает обрезанное (Trim) исходное имя
    /// </summary>
    public static string GetNameBeforeStandard(string fullName)
    {
      int index = GetStandardKeywordIndex(fullName);
      return index > 0 ? fullName.Substring(0, index).Trim() : fullName.Trim();
    }

    /// <summary>
    /// Находит индекс первого упоминания любого стандарта из <see cref="StandardConstants.Standards"/> в имени (без учёта регистра). 
    /// <br/>Если ни один стандарт не найден, возвращает -1
    /// </summary>
    public static int GetStandardKeywordIndex(string fullName)
    {
      return StandardConstants.Standards
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
    /// Возвращает ключевое слово стандарта (например: <c>"ГОСТ"</c>, <c>"ISO"</c>), 
    /// <br/> если строка начинается с одного из стандартов в <see cref="StandardConstants.Standards"/>.
    /// </summary>
    public static string GetStandard(string fullStandard)
    {
      if (string.IsNullOrWhiteSpace(fullStandard))
        return string.Empty;

      return StandardConstants.Standards.FirstOrDefault(x => fullStandard.StartsWith(x, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }

    /// <summary>
    /// Приводит полное имя материала к корректному формату:
    ///<br/>- Первая буква имени — заглавная.
    ///<br/>- Стандарт выделяется отдельно:
    ///<br/>- Для стандартов из <see cref="StandardConstants.StandardsTitleCase"/> первая буква заглавная.
    ///<br/>- Для специальных стандартов <c>"SNiP"</c> и <c>"SANPIN"</c> используется особое форматирование.
    ///<br/>- Все остальные стандарты приводятся к верхнему регистру.
    /// </summary>   
    public static string FormatFullName(string fullName)
    {
      if (string.IsNullOrWhiteSpace(fullName))
        return string.Empty;

      var normalized = fullName.FirstCharToUpper();
      var fullStandard = GetFullStandard(normalized);

      if (!string.IsNullOrEmpty(fullStandard))
      {
        var index = GetStandardKeywordIndex(normalized);
        if (index > 0)
        {
          var standard = GetStandard(fullStandard);
          var prefix = normalized.Substring(0, index).Trim();
          if (StandardConstants.StandardsTitleCase.FirstOrDefault(x => x.Equals(standard, StringComparison.OrdinalIgnoreCase)) is not null)
            return $"{prefix} {standard.FirstCharToUpper()} {fullStandard.Substring(standard.Length)}";
          if (standard.Equals(StandardConstants.SNiP, StringComparison.OrdinalIgnoreCase))
            return $"{prefix} {StandardConstants.SNiP} {fullStandard.Substring(standard.Length)}";
          if (standard.Equals(StandardConstants.SANPIN, StringComparison.OrdinalIgnoreCase))
            return $"{prefix} {StandardConstants.SANPIN} {fullStandard.Substring(standard.Length)}";
          return $"{prefix} {fullStandard.ToUpper()}";
        }
      }
      return normalized;
    }

    /// <summary>Формирует имя и описание документа на основе стандарта и базового имени.</summary>
    public static (string Name, string Description) GetNameAndDescriptionForDocument(string fullStandard, string baseName)
    {
      return (fullStandard + " " + baseName + ". " + CatalogConstants.SUFFIX_DOCUMENT,
              $"ОБОЗНАЧЕНИЕ \t\t{fullStandard} \nНАИМЕНОВАНИЕ \t\t {baseName}. {CatalogConstants.SUFFIX_DOCUMENT}");
    }
  }
}