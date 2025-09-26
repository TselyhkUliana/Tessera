using System.Windows.Input;
using Tessera.App.Command;
using Tessera.PolinomProvider;

namespace Tessera.App
{
  public static class CommandFactory
  {
    public static ICommand CreateCheckAndCreateEntities(ISuggestionProvider suggestionProvider, IReferenceProvider referenceProvider) => new CheckAndCreateEntitiesCommand(suggestionProvider, referenceProvider);
    public static ICommand CreateAddFileForMaterial(IReferenceProvider referenceProvider) => new AddFileForMaterialCommand(referenceProvider);
    public static ICommand CreateAddFileForSortament(IReferenceProvider referenceProvider) => new AddFileForSortamentCommand(referenceProvider);
    public static ICommand CreateRemove(IReferenceProvider referenceProvider) => new RemoveCommand(referenceProvider);
  }
}
