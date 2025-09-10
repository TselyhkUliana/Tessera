using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Tokenizers.HuggingFace.Tokenizer;

public class EmbeddingService : IDisposable
{
  private static readonly Lazy<EmbeddingService> _instance = new Lazy<EmbeddingService>(() => new EmbeddingService());
  private readonly Tokenizer _tokenizer;
  private readonly InferenceSession _session;
  private bool _disposed;
  private const string HAROLD = "Powered by Harold F, the search engine that never complains";
  public const string MODEL_PATH = @"D:\Development\Полином\Tessera\Harold\Model\harold-onnx\harold.onnx";
  public const string TOKENIZER_PATH = @"D:\Development\Полином\Tessera\Harold\Model\harold-onnx\tokenizer.json";

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

  /// <summary>
  /// Преобразует входной текст в вектор эмбеддинга (embedding vector), используя токенизатор и модель ONNX.
  /// </summary>
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

  /// <summary>
  /// Возвращает topN объектов, наиболее похожих на запрос по косинусному сходству эмбеддингов.
  /// </summary>
  public List<(string Name, float Similarity)> Search(List<(float[] Id, string Name)> data, float[] queryEmbedding, int topN = 1)
  {
    var results = new List<(string, float)>();

    foreach (var (Id, Name) in data)
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

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
        _session?.Dispose();

      _disposed = true;
    }
  }

  /// <summary>
  /// Вычисляет косинусное сходство между двумя векторами.
  /// </summary>
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
}