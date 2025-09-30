using System.Threading.Tasks;
using Tessera.PolinomProvider;
using Tessera.PolinomProvider.Interface;
using Tessera.SemanticIndexBuilder;
using Tessera.SemanticIndexBuilder.Embeddings;

internal class Program
{
  [STAThread]
  private static void Main(string[] args)
  {
    var embeddingUpdater = PolinomProvider.Instance;
    UpdateElementEmbeddings(embeddingUpdater, new PolinomEmbeddingUpdater()).GetAwaiter().GetResult();
  }

  private static async Task UpdateElementEmbeddings(IEmbeddingProvider embeddingProvider, IElementEmbeddingUpdater elementEmbeddingUpdater)
  {
    var manager = new ElementEmbeddingUpdateManager(embeddingProvider, elementEmbeddingUpdater);
    await manager.Update();
  }
}