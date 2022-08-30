using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpeechToolsBot.UpdateProcessors.CommandHandlers;

internal static class PublicHandler
{
    public static Task StartAsync(ITelegramBotClient client, Message message)
    {
        const string startText = @"Welcome To SpeechTools Bot :D
1.Convert a message (shorter than 100char) To a Voice
2.Convert a voice to a message

`This Bot Is an Open-Source Service Powered By Azure AI`.
***You Can Find The Source*** [Here](https://github.com/MrAliSalehi/SpeechToolsBot)";
        return client.SendTextMessageAsync(message.Chat.Id, startText, ParseMode.Markdown, disableWebPagePreview: true);
    }
}