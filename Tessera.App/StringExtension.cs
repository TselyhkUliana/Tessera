using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.App
{
  public static class StringExtension
  {
    public static string FirstCharToUpper(this string str)
    {
      if (string.IsNullOrEmpty(str))
        return str;
      return char.ToUpper(str[0]) + str.Substring(1);
    }
  }
}
