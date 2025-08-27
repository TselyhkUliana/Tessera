namespace Tessera.App.Extensions
{
  public static class StringExtension
  {
    /// <summary>Преобразует первую букву строки в верхний регистр</summary>
    public static string FirstCharToUpper(this string str)
    {
      if (string.IsNullOrEmpty(str))
        return str;
      return char.ToUpper(str[0]) + str.Substring(1);
    }
  }
}
