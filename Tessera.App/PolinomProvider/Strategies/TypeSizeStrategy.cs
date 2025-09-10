using Ascon.Polynom.Api;
using Tessera.App.PolinomProvider.Utils;
using Tessera.App.PolinomProvider.Utils.Constants;
using Tessera.App.ViewModel;

namespace Tessera.App.PolinomProvider.Strategies
{
  internal class TypeSizeStrategy : ITypeSizeStrategy
  {
    private PolinomApiHelper _apiHelper;

    public TypeSizeStrategy(PolinomApiHelper polinomApiHelper)
    {
      _apiHelper = polinomApiHelper;
    }

    public IElement GetOrCreate(SectionDefinitionViewModel sectionDefinition, string groupName)
    {
      var catalog = _apiHelper.GetCatalog(CatalogConstants.CATALOG_TYPE_SIZE);
      var group = _apiHelper.FindGroupByName(catalog.Groups, g => g.Groups, groupName) ?? catalog.CreateGroup(groupName);
      var element = group.Elements.Where(e => e.Name.Equals(sectionDefinition.TypeSizeViewModel.TypeSize, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      element ??= group.CreateElement(sectionDefinition.TypeSizeViewModel.TypeSize);
      element.Applicability = Applicability.Allowed;
      return element;
    }

    public void FillProperties()
    {
      throw new NotImplementedException();
    }
  }
}
