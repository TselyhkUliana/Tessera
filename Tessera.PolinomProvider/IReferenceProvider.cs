using Tessera.PolinomProvider.Model;

namespace Tessera.PolinomProvider
{
  public interface IReferenceProvider
  {
    void EnsureEntitiesExist(SectionDefinition sectionDefinition);
    void AttachFileToDocument(string fileName, byte[] fileBody, string elementName, string catalog);
    //List<string> Test();
  }
}
