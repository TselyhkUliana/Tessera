using Tessera.PolinomProvider.Model;

namespace Tessera.PolinomProvider.Interface
{
  public interface IReferenceProvider
  {
    void EnsureEntitiesExist(SectionDefinition sectionDefinition);
    //void AttachFileToDocument(string fileName, byte[] fileBody, string elementName, string catalog);
    Task<Elements> LoadElementsWithEmbeddingAsync();
  }
}
