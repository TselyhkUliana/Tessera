using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.App.Model
{
  public class SectionDefinition
  {
    /// <summary>Материал</summary>
    public string Material { get; set; }
    /// <summary>Сортамент</summary>
    public string Sortament { get; set; }
    /// <summary>Типоразмер</summary>
    public string TypeSize { get; set; }
    /// <summary>Экземпляр сортамента</summary>
    public string SortamentEx { get; set; }
  }
}
