using System.Diagnostics;
using SpeechToolsBot.Common.ValueObjects;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SpeechToolsBot.UpdateProcessors.CommandHandlers;

internal class MainCommandHandler
{
    private readonly ITelegramBotClient _client;
    private readonly List<StaticCommand> _commands;
    private readonly MethodResponseCacher<string, bool> _methodCache;

    public MainCommandHandler(ITelegramBotClient client)
    {
        _client = client;
        _commands = new List<StaticCommand>
        {
            StaticCommand.From(("/start", tuple => PublicHandler.StartAsync(tuple.client, tuple.message)))
        };
        _methodCache = new MethodResponseCacher<string, bool>(msg => _commands.Any(p => p.Value.command == msg));
    }

    public bool IsCommand(string message) => _methodCache.Invoke(message);

    public async Task ProcessCommandAsync(Message message)
    {
        try
        {
            var command = _commands.FirstOrDefault(p => p.Value.command == message.Text);
            if (command is null)
                return;

            await command.Value.UpdateAction((_client, message));
        }
        catch (Exception e)
        {
            Log.Error(e.Demystify(), nameof(ProcessCommandAsync));
        }
    }
}