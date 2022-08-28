using Telegram.Bot;
using Telegram.Bot.Types;

namespace WorkerService1.UpdateProcessors;

internal class TextMessage
{
    private readonly ITelegramBotClient _client;

    public TextMessage(ITelegramBotClient client)
    {
        _client = client;
    }
    
    public async Task ProcessMessageAsync(Message message,CancellationToken ct = default)
    {
        if (message.From is null || message.Text is null or "")
            return;
        
        await _client.SendTextMessageAsync(message.From.Id,message.Text,cancellationToken:ct);
    }
}