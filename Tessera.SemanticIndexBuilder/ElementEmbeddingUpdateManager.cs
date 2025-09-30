using System.Threading.Tasks;
using Tessera.PolinomProvider.Interface;

namespace Tessera.SemanticIndexBuilder
{
  internal class ElementEmbeddingUpdateManager
  {
    private readonly IEmbeddingProvider _embeddingProvider;

    public ElementEmbeddingUpdateManager(IEmbeddingProvider embeddingProvider, IElementEmbeddingUpdater elementEmbeddingUpdater)
    {
      _embeddingProvider = embeddingProvider;
      Updater = elementEmbeddingUpdater;
    }

    public IElementEmbeddingUpdater Updater { get; private set; }

    public async Task Update()
    {
     await Updater.UpdateEmbeddings(_embeddingProvider);
    }
  }
}
