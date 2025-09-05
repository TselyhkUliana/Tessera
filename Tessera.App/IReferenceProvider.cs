using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.App.ViewModel;

namespace Tessera.App
{
  public interface IReferenceProvider
  {
    void EnsureEntitiesExist(IEnumerable<SectionDefinitionViewModel> sectionDefinitionViewModels);
    void AttachFileToDocument(string fileName, byte[] fileBody, string elementName, string catalog);
  }
}
