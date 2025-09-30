using Tessera.PolinomProvider.Interface;

namespace Tessera.SemanticIndexBuilder
{
  internal interface IElementEmbeddingUpdater
  {
    Task UpdateEmbeddings(IEmbeddingProvider embeddingProvider);
  }
}