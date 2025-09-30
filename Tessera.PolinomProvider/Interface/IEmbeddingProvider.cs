namespace Tessera.PolinomProvider.Interface
{
  public interface IEmbeddingProvider
  {
    bool EnsureVectorConceptExists();
    void CreateElementEmbedding(string location, string embedding);
    Task<List<(string Name, string Location)>> LoadElementsForEmbeddingAsync();
  }
}