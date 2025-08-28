using Ascon.Polynom.Api;
using Ascon.Polynom.Login;
using Tessera.App.PolinomHandlers.Strategies;
using Tessera.App.PolinomHandlers.Utils;
using Tessera.App.PolinomHandlers.Utils.Constants;
using Tessera.App.ViewModel;
using TransactionManager = Tessera.App.PolinomHandlers.Utils.TransactionManager;

namespace Tessera.App.PolinomHandlers
{
  internal class PolinomHandler
  {
    private static readonly Lazy<PolinomHandler> _instance = new(() => new PolinomHandler());
    private readonly ISession _session;
    private readonly IMaterialStrategy _materialStrategy;
    private readonly ISortamentStrategy _sortamentStrategy;
    private readonly ITypeSizeStrategy _typeSizeStrategy;
    private readonly ISortamentExStrategy _sortamentExStrategy;
    private readonly TransactionManager _transactionManager;
    private readonly PolinomApiHelper _polinomApiHelper;
    private (string FileName, byte[] FileBody) _pendingMaterialFile;
    private (string FileName, byte[] FileBody) _pendingSortamentFile;

    private PolinomHandler()
    {
      LoginManager.TryOpenSession(Guid.Parse(CatalogConstants.POLYNOM_CLIENT_ID), out _session);
      _transactionManager = new TransactionManager(_session);
      _polinomApiHelper = new PolinomApiHelper(_session, _transactionManager);
      _materialStrategy = new MaterialStrategy(_polinomApiHelper);
      _sortamentStrategy = new SortamentStrategy(_polinomApiHelper);
      _typeSizeStrategy = new TypeSizeStrategy(_polinomApiHelper);
      _sortamentExStrategy = new SortamentExStrategy(_polinomApiHelper);
    }

    public static PolinomHandler Instance => _instance.Value;
    public event EventHandler<FileAttachmentEventArgs> MaterialFilePending;
    public event EventHandler<FileAttachmentEventArgs> SortamentFilePending;

    public void EnsureEntitiesExist(IEnumerable<SectionDefinitionViewModel> sectionDefinitionViewModels)
    {
      _transactionManager.ApplyChanges(() =>
      {
        var sectionDefinition = sectionDefinitionViewModels.First();
        var material = _materialStrategy.GetOrCreate(sectionDefinition);
        var sortament = _sortamentStrategy.GetOrCreate(sectionDefinition);
        var typeSize = _typeSizeStrategy.GetOrCreate(sectionDefinition, sortament.Name);
        var sortamentEx = _sortamentExStrategy.GetOrCreate(sortament);

        _polinomApiHelper.CreateLink(sortament, material, LinkConstants.LINK_SORTAMENT_MATERIAL);
        _polinomApiHelper.CreateLink(typeSize, sortament, LinkConstants.LINK_TYPESIZE_SORTAMENT);
        _polinomApiHelper.CreateLink(sortamentEx, sortament, LinkConstants.LINK_SORTAMENTEX_SORTAMENT);
        _polinomApiHelper.CreateLink(sortamentEx, material, LinkConstants.LINK_SORTAMENTEX_MATERIAL);
        _polinomApiHelper.CreateLink(sortamentEx, typeSize, LinkConstants.LINK_SORTAMENTEX_TYPE_SIZE);

        sortamentEx.Evaluate();
        sectionDefinition.SortamentEx = sortamentEx.Name;
        _polinomApiHelper.LinksTest(sortament);
        MaterialFilePending?.Invoke(this, new FileAttachmentEventArgs(material, _pendingMaterialFile.FileBody, _pendingMaterialFile.FileName, CatalogConstants.CATALOG_MATERIAL));
        SortamentFilePending?.Invoke(this, new FileAttachmentEventArgs(sortament, _pendingSortamentFile.FileBody, _pendingSortamentFile.FileName, CatalogConstants.CATALOG_SORTAMENT));
      });
    }

    public void AttachFileToDocument(string fileName, byte[] fileBody, string elementName, string catalog)
    {
      if (catalog is CatalogConstants.CATALOG_MATERIAL)
      {
        _pendingMaterialFile = (fileName, fileBody);
        MaterialFilePending += AttachFileToDocument;
      }
      else if (catalog is CatalogConstants.CATALOG_SORTAMENT)
      {
        _pendingSortamentFile = (fileName, fileBody);
        SortamentFilePending += AttachFileToDocument;
      }
    }

    private void AttachFileToDocument(object sender, FileAttachmentEventArgs eventArgs) => _polinomApiHelper.AttachFile(eventArgs.Element, eventArgs.FileName, eventArgs.FileBody, eventArgs.DocumentGroupName);
  }
}