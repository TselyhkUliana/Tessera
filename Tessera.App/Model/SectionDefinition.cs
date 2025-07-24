using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.App.Model
{
  public class SectionDefinition
  {
    /// <summary>Типоразмер/// </summary>
    public string TypeSize { get; set; }
    /// <summary>Сортамент/// </summary>
    public string SectionProfile { get; set; }
    /// <summary>Материал/// </summary>
    public string Material { get; set; }
    /// <summary>Экземпляр сортамента/// </summary>
    public string SectionInstance { get; set; }
  }
}
