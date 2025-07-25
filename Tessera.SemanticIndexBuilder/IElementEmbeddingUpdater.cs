using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.SemanticIndexBuilder
{
  internal interface IElementEmbeddingUpdater
  {
    public void UpdateEmbeddings(string DatabaseName);
  }
}
