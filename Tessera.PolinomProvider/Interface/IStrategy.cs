using Ascon.Polynom.Api;
using Tessera.PolinomProvider.Model;

namespace Tessera.PolinomProvider.Interface
{
  public interface IMaterialStrategy
  {
    IElement GetOrCreate(SectionDefinition sectionDefinition);
  }

  public interface ISortamentStrategy
  {
    IElement GetOrCreate(SectionDefinition sectionDefinition);
  }

  public interface ITypeSizeStrategy
  {
    IElement GetOrCreate(string typeSizeName, TypeSizeData typeSizeData, IElement sortament);
  }

  public interface ISortamentExStrategy
  {
    IElement GetOrCreate(IElement sortament, IElement material);
  }
}
