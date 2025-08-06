using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.App
{
  internal static class EntityNameHelper
  {
    public static string GetNameBeforeStandard(string fullName)
    {
      int index = GetStandardKeywordIndex(fullName);
      return index > 0 ? fullName.Substring(0, index).Trim() : fullName.Trim();
    }

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
