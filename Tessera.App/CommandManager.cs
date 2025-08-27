using System.Windows.Input;
using Tessera.App.Command;

namespace Tessera.App
{
  class CommandManager
  {
    private readonly Dictionary<string, ICommand> _commands;

    public CommandManager(ISuggestionProvider suggestionProvider)
    {
      _commands = new Dictionary<string, ICommand>
      {
        [nameof(CheckAndCreateEntitiesCommand)] = CommandFactory.CreateCheckAndCreateEntities(suggestionProvider),
        [nameof(AddFileForMaterialCommand)] = CommandFactory.CreateAddFileForMaterial(),
        [nameof(AddFileForSortamentCommand)] = CommandFactory.CreateAddFileForSortament(),
        [nameof(RemoveCommand)] = CommandFactory.CreateRemove()
      };
    }

    public ICommand this[string commandName] => _commands.TryGetValue(commandName, out var command) ? command : throw new KeyNotFoundException($"Не найдена команда \"{commandName}\"");

    public T Get<T>() where T : ICommand
    {
      var key = typeof(T).Name;
      return _commands.TryGetValue(key, out var command) ? (T)command : throw new KeyNotFoundException($"Не найдена команда \"{key}\"");
    }
  }
}