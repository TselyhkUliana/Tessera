using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Tokenizers.HuggingFace.Tokenizer;

public class Item
{
  public string Id { get; set; }
  public string Text { get; set; }
  public float[] Embedding { get; set; }
}

public class Test
{
  public const string MODEL_PATH = @"D:\Development\Полином\Tessera\Harold\Model\bge-m3-onnx\model.onnx";
  public const string TOKENIZER_PATH = @"D:\Development\Полином\Tessera\Harold\Model\bge-m3-onnx\tokenizer.json";

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

    var database = new List<Item>();
    var test = "Анод ДПРХХ 2,0х300х500 М1";
    GetTextEmbedding(tokenizer, session, test, out var results2, out var embedding2);
    for (int i = 0; i < materials.Length; i++)
    {
      string text = materials[i];

      IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results;
      GetTextEmbedding(tokenizer, session, text, out results, out var embedding);

      //Normalizer.Normalize(text);
      database.Add(new Item
      {
        Id = $"item{i + 1}",
        Text = text,
        Embedding = embedding
      });
    }

    Search(database, embedding2).ForEach(x => Console.WriteLine(x.item.Text));
    Console.ReadKey();
  }


  public IEnumerable<(int id, float[] embedding)> GetTextEmbeddings(IEnumerable<(int Id, string Name)> values)
  {
    var test = values.ToList();
    var tokenizer = Tokenizer.FromFile(TOKENIZER_PATH);
    using var session = new InferenceSession(MODEL_PATH);
    var count = test.Count;
    for (int i = 0; i < count; i++)
    {
      Console.Write($"\rСоздано векторов {i + 1} из {count}");
      GetTextEmbedding(tokenizer, session, test[i].Name, out var results, out var embedding);
      yield return (test[i].Id, embedding);
    }
    Console.WriteLine();
  }

  public void GetTextEmbedding(string text, out float[] embedding)
  {
    var tokenizer = Tokenizer.FromFile(@"D:\Development\Полином\Tessera\Harold\Model\bge-m3-onnx\tokenizer.json");
    using var session = new InferenceSession(@"D:\Development\Полином\Tessera\Harold\Model\bge-m3-onnx\model.onnx");
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
    IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
    var outputTensor = results.First().AsTensor<float>();

    embedding = outputTensor.ToArray();
  }

  private void GetTextEmbedding(Tokenizer tokenizer, InferenceSession session, string text, out IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results, out float[] embedding)
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

  public List<(Item item, float similarity)> Search(List<Item> database, float[] queryEmbedding, int topN = 1)
  {
    var results = new List<(Item, float)>();

    foreach (var item in database)
    {
      var sim = CosineSimilarity(item.Embedding, queryEmbedding);
      results.Add((item, sim));
    }

    return results.OrderByDescending(x => x.Item2).Take(topN).ToList();
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
}