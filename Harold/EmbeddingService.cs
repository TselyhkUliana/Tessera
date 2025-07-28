using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Tokenizers.HuggingFace.Tokenizer;

public class EmbeddingService : IDisposable
{
  private static readonly Lazy<EmbeddingService> _instance = new Lazy<EmbeddingService>(() => new EmbeddingService());
  private readonly Tokenizer _tokenizer;
  private readonly InferenceSession _session;
  private bool _disposed;
  public const string MODEL_PATH = @"D:\Development\Полином\Tessera\Harold\Model\bge-m3-onnx\model.onnx";
  public const string TOKENIZER_PATH = @"D:\Development\Полином\Tessera\Harold\Model\bge-m3-onnx\tokenizer.json";

  private EmbeddingService()
  {
    _tokenizer = Tokenizer.FromFile(TOKENIZER_PATH);
    _session = new InferenceSession(MODEL_PATH);
  }

  public static EmbeddingService Instance => _instance.Value;

  ~EmbeddingService()
  {
    Dispose(false);
  }

  public void GetTextEmbedding(string text, out float[] embedding)
  {
    var encoding = _tokenizer.Encode(text, true);
    var inputIds = encoding.Encodings.First().Ids.Select(id => (long)id).ToArray();
    var attentionMaskRaw = encoding.Encodings.First().AttentionMask;
    var attentionMask = (attentionMaskRaw.Count > 0)
        ? attentionMaskRaw.Select(i => (long)i).ToArray()
        : inputIds.Select(id => id == 0 ? 0L : 1L).ToArray();

    var inputIdsTensor = new DenseTensor<long>(new[] { 1, inputIds.Length });
    var attentionMaskTensor = new DenseTensor<long>(new[] { 1, attentionMask.Length });

    for (int j = 0; j < inputIds.Length; j++)
    {
      inputIdsTensor[0, j] = inputIds[j];
      attentionMaskTensor[0, j] = attentionMask[j];
    }

    var inputs = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
        NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
    };
    IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);
    var outputTensor = results.First().AsTensor<float>();

    embedding = outputTensor.ToArray();
  }

  public List<(string name, float similarity)> Search(List<(float[] Id, string Name)> database, float[] queryEmbedding, int topN = 1)
  {
    var results = new List<(string, float)>();

    foreach (var (Id, Name) in database)
    {
      var sim = CosineSimilarity(Id, queryEmbedding);
      results.Add((Name, sim));
    }

    return results.OrderByDescending(x => x.Item2).Take(topN).ToList();
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  private float CosineSimilarity(float[] vectorA, float[] vectorB)
  {
    if (vectorA.Length != vectorB.Length)
      throw new ArgumentException("Векторы должны быть одинаковой длины");

    var dot = 0f;
    var normA = 0f;
    var normB = 0f;

    for (int i = 0; i < vectorA.Length; i++)
    {
      dot += vectorA[i] * vectorB[i];
      normA += vectorA[i] * vectorA[i];
      normB += vectorB[i] * vectorB[i];
    }

    if (normA == 0 || normB == 0)
      return 0;

    return dot / (float)(Math.Sqrt(normA) * Math.Sqrt(normB));
  }

  private void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
        _session?.Dispose();

      _disposed = true;
    }
  }

  #region Test
  private void GetTextEmbeddingTest(Tokenizer tokenizer, InferenceSession session, string text, out IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results, out float[] embedding)
  {
    var encoding = tokenizer.Encode(text, true);
    var inputIds = encoding.Encodings.First().Ids.Select(id => (long)id).ToArray();
    var attentionMaskRaw = encoding.Encodings.First().AttentionMask;
    var attentionMask = (attentionMaskRaw.Count > 0)
        ? attentionMaskRaw.Select(i => (long)i).ToArray()
        : inputIds.Select(id => id == 0 ? 0L : 1L).ToArray();

    var inputIdsTensor = new DenseTensor<long>(new[] { 1, inputIds.Length });
    var attentionMaskTensor = new DenseTensor<long>(new[] { 1, attentionMask.Length });

    for (int j = 0; j < inputIds.Length; j++)
    {
      inputIdsTensor[0, j] = inputIds[j];
      attentionMaskTensor[0, j] = attentionMask[j];
    }

    var inputs = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
        NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
    };
    results = session.Run(inputs);
    var outputTensor = results.First().AsTensor<float>();

    embedding = outputTensor.ToArray();
  }

  private List<(ItemTest item, float similarity)> SearchTest(List<ItemTest> database, float[] queryEmbedding, int topN = 1)
  {
    var results = new List<(ItemTest, float)>();

    foreach (var item in database)
    {
      var sim = CosineSimilarity(item.Embedding, queryEmbedding);
      results.Add((item, sim));
    }

    return results.OrderByDescending(x => x.Item2).Take(topN).ToList();
  }

  public void Test2()
  {
    var tokenizer = Tokenizer.FromFile(@"D:\Development\Полином\Tessera\Harold\Model\bge-m3-onnx\tokenizer.json");
    using var session = new InferenceSession(@"D:\Development\Полином\Tessera\Harold\Model\bge-m3-onnx\model.onnx");

    var materials = new[]
    {
      "Анод Зл 99,99 Ан АН2 2,00х150х300 ГОСТ 25475-2015",
      "Анод ГОВНХ 80х35х800 МД НПАН ГОСТ 2132-2015",
      "Анод ГПРНХ 4х100х900 МД НПА1 ГОСТ 2132-2015",
      "Анод ГПРПХ 12х100х800 МД НПА1 ГОСТ 2132-2015",
      "Анод ГПРНХ 4х100х500 МД НПАН ГОСТ 2132-2015",
      "Анод ГПРНХ 4х100х700 КД НПА2 ГОСТ 2132-2015",
      "Анод ГПРХХ 10х200х450 Ц0 ГОСТ 1180-2021",
      "Анод ДПРХХ 5х140х460 М1 ГОСТ 767-91",
      "Анод ГПРХХ 10х600х2000 АМФ ГОСТ 767-91",
      "Анод ДПРХХ 7,0х300х800 М1 ГОСТ 767-2020",
      //"Анод ДПРХХ 2,0х300х500 М1 ГОСТ 767-2020",
      "Анод ГПРХХ 10,0х600х1000 АМФ ГОСТ 767-2020",
      "Анод Ср 99,99 Ан АН2 2х100х200 ГОСТ 25474-2015"
    };

    var database = new List<ItemTest>();
    var test = "Анод ДПРХХ 2,0х300х500 М1";
    GetTextEmbeddingTest(tokenizer, session, test, out var results2, out var embedding2);
    for (int i = 0; i < materials.Length; i++)
    {
      string text = materials[i];

      IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results;
      GetTextEmbeddingTest(tokenizer, session, text, out results, out var embedding);

      //Normalizer.Normalize(text);
      database.Add(new ItemTest
      {
        Id = $"item{i + 1}",
        Text = text,
        Embedding = embedding
      });
    }

    SearchTest(database, embedding2).ForEach(x => Console.WriteLine(x.item.Text));
    Console.ReadKey();
  }

  #endregion
}

public class ItemTest
{
  public string Id { get; set; }
  public string Text { get; set; }
  public float[] Embedding { get; set; }
}