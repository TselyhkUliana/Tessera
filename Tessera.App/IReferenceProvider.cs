using Tessera.App.ViewModel;

namespace Tessera.App
{
  public interface IReferenceProvider
  {
    void EnsureEntitiesExist(SectionDefinitionViewModel sectionDefinition);
    void AttachFileToDocument(string fileName, byte[] fileBody, string elementName, string catalog);
    //List<string> Test();
  }
}
