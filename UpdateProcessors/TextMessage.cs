using System.Diagnostics;
using SpeechToolsBot.ApiCalls;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SpeechToolsBot.UpdateProcessors;

internal class TextMessage
{
    private readonly ITelegramBotClient _client;
    private readonly TextToSpeechApi _textToSpeechApi;

    public TextMessage(ITelegramBotClient client, TextToSpeechApi textToSpeechApi)
    {
        _client = client;
        _textToSpeechApi = textToSpeechApi;
    }


    public async Task ProcessMessageAsync(Message message, CancellationToken ct = default)
    {
        if (message.Text is null or "")
            return;

        if (message.Text.Length > 100)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, "Your Message Must Be Shorter Than 100 Character.", cancellationToken: ct);

            return;
        }

        var apiResponse = await _textToSpeechApi.GetAudioAsync(message.Text);
        if (apiResponse is null)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, "Cant Convert Your Text Right Now.\nPlease Contact Admin.", cancellationToken: ct);
            return;
        }

        try
        {
            await using var stream = new MemoryStream();
            await stream.WriteAsync(apiResponse.AudioData, ct);
            //stream.Position = 0;

            await _client.SendVoiceAsync(message.Chat.Id, stream!, $"Status :{apiResponse.Reason}", duration: apiResponse.AudioDuration.Seconds, cancellationToken: ct);
        }
        catch (OverflowException)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, "Your Message Was a little Big.\nTry a Shorter Message!", cancellationToken: ct);
        }
        catch (Exception e)
        {
            Log.Error(e.Demystify(), nameof(ProcessMessageAsync));
            await _client.SendTextMessageAsync(message.Chat.Id, "Where Was Some Problem :( \n Try Again Later", cancellationToken: ct);
        }
    }
}