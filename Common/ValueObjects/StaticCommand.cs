using SpeechToolsBot.Common.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types;
using ValueOf;

namespace SpeechToolsBot.Common.ValueObjects;

public class StaticCommand : ValueOf<(string command, Func<(ITelegramBotClient client, Message message), Task> UpdateAction), StaticCommand>
{
    protected override void Validate()
    {
        if (!Value.command.StartsWith("/"))
        {
            throw new InvalidCommandException($"Command Should Start With /,Given Command Was :{Value.command}");
        }
    }
}