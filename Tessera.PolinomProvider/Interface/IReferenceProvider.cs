using Tessera.PolinomProvider.Model;
using Tessera.PolinomProvider.Utils;

namespace Tessera.PolinomProvider.Interface
{
  public interface IReferenceProvider
  {
    TransactionManager TransactionManager { get; }
    void EnsureEntitiesExist(SectionDefinition sectionDefinition);
    //void AttachFileToDocument(string fileName, byte[] fileBody, string elementName, string catalog);
    Task<Elements> LoadElementsWithEmbeddingAsync();
  }
}
