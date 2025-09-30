namespace Tessera.PolinomProvider.Extensions
{
  public static class FloatExtension
  {
    public static float[] ParseEmbeddingVector(this string str)
    {
      if (string.IsNullOrEmpty(str))
        return Array.Empty<float>();

      return str.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => float.Parse(x, System.Globalization.CultureInfo.InvariantCulture))
                .ToArray()!; ;
    }
  }
}
