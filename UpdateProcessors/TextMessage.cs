using System.Diagnostics;
using SpeechToolsBot.ApiCalls;
using SpeechToolsBot.UpdateProcessors.CommandHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpeechToolsBot.UpdateProcessors;

internal class TextMessage
{
    private readonly ITelegramBotClient _client;
    private readonly MainCommandHandler _mainCommandHandler;
    private readonly TextToSpeechApi _textToSpeechApi;

    public TextMessage(ITelegramBotClient client, TextToSpeechApi textToSpeechApi, MainCommandHandler mainCommandHandler)
    {
        _client = client;
        _textToSpeechApi = textToSpeechApi;
        _mainCommandHandler = mainCommandHandler;
    }


    public async Task ProcessMessageAsync(Message message, CancellationToken ct = default)
    {
        if (message.Text is null or "")
            return;

        if (_mainCommandHandler.IsCommand(message.Text))
        {
            await _mainCommandHandler.ProcessCommandAsync(message);
            return;
        }

        if (message.Text.Length is > 100 or < 3)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, "Your Message Must Be Shorter Than 100 Character.", cancellationToken: ct);
            return;
        }

        await _client.SendTextMessageAsync(message.Chat.Id, "<i>Processing Your Audio...</i>", ParseMode.Html, cancellationToken: ct);

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

            stream.Position = 0;
            if (stream is null)
                throw new NullReferenceException();

            Log.Information("stream Position:[{Pos}]\nCapacity:[{Str}]", stream.Position, stream.Capacity);
            await _client.SendVoiceAsync(message.Chat.Id, stream, $"Status : {apiResponse.Reason}\nDuration:{apiResponse.AudioDuration:g}", duration: apiResponse.AudioDuration.Seconds, cancellationToken: ct);
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