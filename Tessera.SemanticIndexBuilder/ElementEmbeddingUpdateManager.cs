using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.SemanticIndexBuilder
{
  internal class ElementEmbeddingUpdateManager
  {
    private readonly string _databaseName;

    public ElementEmbeddingUpdateManager(string databaseName, IElementEmbeddingUpdater updateElement)
    {
      _databaseName = databaseName;
      Updater = updateElement;
    }

    public IElementEmbeddingUpdater Updater { get; private set; }

    public void Update()
    {
      Updater.UpdateEmbeddings(_databaseName);
    }
  }
}
