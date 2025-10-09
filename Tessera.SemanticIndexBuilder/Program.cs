using System.Windows;
using Tessera.PolinomProvider;
using Tessera.PolinomProvider.Interface;
using Tessera.SemanticIndexBuilder;
using Tessera.SemanticIndexBuilder.Embeddings;

internal class Program
{
  [STAThread]
  private static void Main()
  {
    var tcs = new TaskCompletionSource();

    var thread = new Thread(async () =>
    {
      try
      {
        var provider = PolinomProvider.Instance;
        await UpdateElementEmbeddingsAsync(provider, new PolinomEmbeddingUpdater());
        tcs.SetResult();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
        tcs.SetException(ex);
      }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();

    tcs.Task.GetAwaiter().GetResult();
  }

  private static async Task UpdateElementEmbeddingsAsync(IEmbeddingProvider embeddingProvider, IElementEmbeddingUpdater elementEmbeddingUpdater)
  {
    var manager = new ElementEmbeddingUpdateManager(embeddingProvider, elementEmbeddingUpdater);
    await manager.Update();
  }
}