using System.Windows.Input;
using Tessera.App.Command;

namespace Tessera.App
{
  public static class CommandFactory
  {
    public static ICommand CreateCheckAndCreateEntities(ISuggestionProvider suggestionProvider) => new CheckAndCreateEntitiesCommand(suggestionProvider);
    public static ICommand CreateAddFileForMaterial() => new AddFileForMaterialCommand();
    public static ICommand CreateAddFileForSortament() => new AddFileForSortamentCommand();
    public static ICommand CreateRemove() => new RemoveCommand();
  }
}
