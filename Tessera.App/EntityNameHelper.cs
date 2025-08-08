namespace Tessera.App
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
         .Where(index => index > 0)
         .DefaultIfEmpty(-1)
         .First();
    }
  }
}
