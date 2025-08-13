using Ascon.Polynom.Api;
using Tessera.App.ViewModel;

namespace Tessera.App.PolinomHandlers
{
  public interface IMaterialStrategy
  {
    IElement GetOrCreate(SectionDefinitionViewModel sectionDefinition);
  }

  public interface ISortamentExStrategy
  {
    IElement GetOrCreate(IElement sortament);
  }

  public interface ISortamentStrategy
  {
    IElement GetOrCreate(SectionDefinitionViewModel sectionDefinition);
  }

  public interface ITypeSizeStrategy
  {
    IElement GetOrCreate(SectionDefinitionViewModel sectionDefinition, string groupName);
  }
}
