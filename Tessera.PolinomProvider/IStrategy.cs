using Ascon.Polynom.Api;
using Tessera.PolinomProvider.Model;

namespace Tessera.PolinomProvider
{
  public interface IMaterialStrategy
  {
    IElement GetOrCreate(SectionDefinition sectionDefinition);
  }

  public interface ISortamentExStrategy
  {
    IElement GetOrCreate(IElement sortament);
  }

  public interface ISortamentStrategy
  {
    IElement GetOrCreate(SectionDefinition sectionDefinition);
  }

  public interface ITypeSizeStrategy
  {
    IElement GetOrCreate(string typeSizeName, TypeSizeData typeSizeData, IElement sortament);
  }
}
