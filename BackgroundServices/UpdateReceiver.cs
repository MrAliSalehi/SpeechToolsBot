using System.Diagnostics;
using SpeechToolsBot.Common.Contracts;
using SpeechToolsBot.UpdateProcessors;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpeechToolsBot.BackgroundServices;

internal class UpdateReceiver : BackgroundService, IUpdateReceiver
{
    private readonly AudioMessage _audioMessage;
    private readonly ITelegramBotClient _client;
    private readonly TextMessage _textMessageHandler;

    public UpdateReceiver(ITelegramBotClient client, TextMessage textMessageHandler, AudioMessage audioMessage)
    {
        _client = client;
        _textMessageHandler = textMessageHandler;
        _audioMessage = audioMessage;
    }

    private Task HandleUpdatesAsync(Update update, CancellationToken cancellationToken)
    {
        return update.Type switch
        {
            UpdateType.Message when update.Message?.Voice is not null || update.Message?.Audio is not null || update.Message?.Document is not null =>
                _audioMessage.ProcessAudioAsync(update.Message, cancellationToken),

            UpdateType.Message when update.Message is not null =>
                _textMessageHandler.ProcessMessageAsync(update.Message, cancellationToken),

            _ => Task.CompletedTask
        };
    }

    public static Task HandleErrorsAsync(Exception exception)
    {
        Log.Error(exception.Demystify(), nameof(HandleErrorsAsync));
        return Task.CompletedTask;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var getMe = await _client.GetMeAsync(cancellationToken);
        Log.Information("Bot[{FName}:{LName}] Started ON :{UserName}",
            getMe.FirstName, getMe.LastName, getMe.Username);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _client.ReceiveAsync((_, update, arg3) => HandleUpdatesAsync(update, arg3), (_, exception, _) => HandleErrorsAsync(exception), StaticVariables.ReceiverOptions, cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Demystify(), nameof(ExecuteTask));
        }
    }
}