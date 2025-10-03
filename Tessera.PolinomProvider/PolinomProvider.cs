using Ascon.Polynom.Api;
using Ascon.Polynom.Login;
using System.Diagnostics;
using System.Threading.Tasks;
using Tessera.PolinomProvider.Constants;
using Tessera.PolinomProvider.Interface;
using Tessera.PolinomProvider.Model;
using Tessera.PolinomProvider.Strategies;
using Tessera.PolinomProvider.Utils;
using TransactionManager = Tessera.PolinomProvider.Utils.TransactionManager;

namespace Tessera.PolinomProvider
{
  public class PolinomProvider : IReferenceProvider, IEmbeddingProvider
  {
    private static readonly Lazy<PolinomProvider> _instance = new(() => new PolinomProvider());
    private readonly ISession _session;
    private readonly IMaterialStrategy _materialStrategy;
    private readonly ISortamentStrategy _sortamentStrategy;
    private readonly ITypeSizeStrategy _typeSizeStrategy;
    private readonly ISortamentExStrategy _sortamentExStrategy;
    private readonly TransactionManager _transactionManager;
    private readonly PolinomApiHelper _polinomApiHelper;
    private readonly Dictionary<string, PropertyType> _cachedProperties;
    //private (string FileName, byte[] FileBody) _pendingMaterialFile;
    //private (string FileName, byte[] FileBody) _pendingSortamentFile;

    private PolinomProvider()
    {
      LoginManager.TryOpenSession(Guid.Parse(CatalogConstants.POLYNOM_CLIENT_ID), out _session);
      _transactionManager = new TransactionManager(_session);
      _polinomApiHelper = new PolinomApiHelper(_session, _transactionManager);
      _materialStrategy = new MaterialStrategy(_polinomApiHelper);
      _sortamentStrategy = new SortamentStrategy(_polinomApiHelper);
      _typeSizeStrategy = new TypeSizeStrategy(_polinomApiHelper);
      _sortamentExStrategy = new SortamentExStrategy(_polinomApiHelper);
      _cachedProperties = _polinomApiHelper.GetProperties();
    }

    public TransactionManager TransactionManager => _transactionManager;
    public static PolinomProvider Instance => _instance.Value;
    public event EventHandler<FileAttachmentEventArgs> MaterialFilePending;
    public event EventHandler<FileAttachmentEventArgs> SortamentFilePending;
    public Dictionary<string, PropertyType> AllDimensionProperties => _cachedProperties;

    public async Task<List<string>> GetPropertiesForTypeSize(string similarSortament) => await _polinomApiHelper.GetPropertiesForTypeSizeInternal(similarSortament);

    public void EnsureEntitiesExist(SectionDefinition sectionDefinition)
    {
      var material = _materialStrategy.GetOrCreate(sectionDefinition);
      var sortament = _sortamentStrategy.GetOrCreate(sectionDefinition);
      var typeSize = _typeSizeStrategy.GetOrCreate(sectionDefinition.TypeSize, sectionDefinition.TypeSizeData, sortament);
      var sortamentEx = _sortamentExStrategy.GetOrCreate(sortament, material);
      _polinomApiHelper.CreateLink(sortament, material, LinkConstants.LINK_SORTAMENT_MATERIAL);
      _polinomApiHelper.CreateLink(typeSize, sortament, LinkConstants.LINK_TYPESIZE_SORTAMENT);
      _polinomApiHelper.CreateLink(sortamentEx, sortament, LinkConstants.LINK_SORTAMENTEX_SORTAMENT);
      _polinomApiHelper.CreateLink(sortamentEx, material, LinkConstants.LINK_SORTAMENTEX_MATERIAL);
      _polinomApiHelper.CreateLink(sortamentEx, typeSize, LinkConstants.LINK_SORTAMENTEX_TYPE_SIZE);

      sortamentEx.Evaluate();
      sectionDefinition.SortamentEx = sortamentEx.Name;
      //_polinomApiHelper.LinksTest(sortament);
      //MaterialFilePending?.Invoke(this, new FileAttachmentEventArgs(material, _pendingMaterialFile.FileBody, _pendingMaterialFile.FileName, CatalogConstants.CATALOG_MATERIAL));
      //SortamentFilePending?.Invoke(this, new FileAttachmentEventArgs(sortament, _pendingSortamentFile.FileBody, _pendingSortamentFile.FileName, CatalogConstants.CATALOG_SORTAMENT));
      //_polinomApiHelper.GetProperties();
    }

    public async Task<Elements> LoadElementsWithEmbeddingAsync()
    {
      return await _polinomApiHelper.LoadElementsWithEmbeddingAsync();
    }

    public bool EnsureVectorConceptExists()
    {
      bool created = false;
      _transactionManager.ApplyChanges(() => created = _polinomApiHelper.EnsureVectorConceptExists());
      return created;
    }

    public Task<IEnumerable<(string Name, string Location)>> LoadElementsForEmbeddingAsync()
    {
      return _polinomApiHelper.LoadElementsForEmbeddingAsync();
    }

    public void CreateElementEmbedding(string location, string embedding)
    {
      _transactionManager.ApplyChanges(() => _polinomApiHelper.CreateElementEmbedding(location, embedding));
    }

    //public void AttachFileToDocument(string fileName, byte[] fileBody, string elementName, string catalog)
    //{
    //  if (catalog is CatalogConstants.CATALOG_MATERIAL)
    //  {
    //    _pendingMaterialFile = (fileName, fileBody);
    //    MaterialFilePending += AttachFileToDocument;
    //  }
    //  else if (catalog is CatalogConstants.CATALOG_SORTAMENT)
    //  {
    //    _pendingSortamentFile = (fileName, fileBody);
    //    SortamentFilePending += AttachFileToDocument;
    //  }
    //}

    //private void AttachFileToDocument(object sender, FileAttachmentEventArgs eventArgs) => _polinomApiHelper.AttachFile(eventArgs.Element, eventArgs.FileName, eventArgs.FileBody, eventArgs.DocumentGroupName);
  }
}