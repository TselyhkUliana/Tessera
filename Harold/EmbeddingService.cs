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
#if DEBUG
  public const string MODEL_PATH = @"D:\Development\Полином\Tessera\Harold\Model\harold-onnx\harold.onnx";
  public const string TOKENIZER_PATH = @"D:\Development\Полином\Tessera\Harold\Model\harold-onnx\tokenizer.json";
#else
  public const string MODEL_PATH = @$"{AppContext.BaseDirectory}Harold\Model\harold-onnx\harold.onnx";
  public const string TOKENIZER_PATH = @$"{AppContext.BaseDirectory}Harold\Model\harold-onnx\tokenizer.json";
#endif

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
    using var results = _session.Run(inputs);
    var outputTensor = results.First().AsTensor<float>(); //[1, 1024]

    int hiddenSize = outputTensor.Dimensions[1];
    embedding = new float[hiddenSize];

    for (int i = 0; i < hiddenSize; i++)
      embedding[i] = outputTensor[0, i];

    //L2-нормализация
    var norm = MathF.Sqrt(embedding.Sum(x => x * x));
    if (norm > 0)
    {
      for (int i = 0; i < embedding.Length; i++)
        embedding[i] /= norm;
    }
  }

  /// <summary>
  /// Возвращает topN объектов, наиболее похожих на запрос по косинусному сходству эмбеддингов.
  /// </summary>
  public List<(string name, float score)> Search(List<(float[] embedding, string name)> data, float[] queryEmbeddingL2, string query, int topN = 10, float threshold = 0.3f)
  {
    var queryLower = string.Join(" ", query.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();

    var resultsL2 = data
        .Select(item =>
        {
          var similarity = CosineSimilarityL2(item.embedding, queryEmbeddingL2);
          var nameLower = item.name.ToLowerInvariant();
          if (nameLower.Contains(queryLower))
            similarity += threshold;

          return (item.name, similarity);
        })
        .OrderByDescending(x => x.similarity)
        .Take(topN)
        .ToList();

    return resultsL2;
  }

  //public List<(string Name, float Score)> Search(List<(float[] Id, string Name)> data, float[] queryEmbedding, string query, int topN = 10, float threshold = 0.7f, float keywordBoost = 0.1f, float semanticWeight = 0.8f, float keywordWeight = 0.2f)
  //{
  //  var results = new List<(string, float)>();
  //  var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

  //  foreach (var (Id, Name) in data)
  //  {
  //    var semanticScore = CosineSimilarityL2(Id, queryEmbedding);

  //    var keywordMatches = words.Count(w => Name.Contains(w, StringComparison.OrdinalIgnoreCase));
  //    var keywordScore = keywordMatches * keywordBoost;
  //    var finalScore = semanticWeight * semanticScore + keywordWeight * keywordScore;

  //    if (finalScore >= threshold)
  //      results.Add((Name, finalScore));
  //  }

  //  return results.OrderByDescending(r => r.Item2).Take(topN).ToList();
  //}

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
  /// Вычисляет косинусное сходство между двумя нормализованными (L2) векторами.
  /// </summary>
  private float CosineSimilarityL2(float[] vectorA, float[] vectorB)
  {
    var dot = 0f;
    for (int i = 0; i < vectorA.Length; i++)
      dot += vectorA[i] * vectorB[i];

    return dot;
  }

  /// <summary>
  /// Вычисляет косинусное сходство между двумя векторами.
  /// </summary>
  private float CosineSimilarity(float[] vectorA, float[] vectorB)
  {
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