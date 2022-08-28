﻿using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkerService1.UpdateProcessors;
using IUpdateReceiver = WorkerService1.Common.Contracts.IUpdateReceiver;

namespace WorkerService1.BackgroundServices;

internal class UpdateReceiver : BackgroundService, IUpdateReceiver
{
    private readonly ITelegramBotClient _client;
    private readonly TextMessage _textMessageHandler;

    public UpdateReceiver(ITelegramBotClient client, TextMessage textMessageHandler)
    {
        _client = client;
        _textMessageHandler = textMessageHandler;
    }

    private Task HandleUpdatesAsync(Update update, CancellationToken cancellationToken)
    {
        return update.Type switch
        {
            UpdateType.Message when (update.Message is not null) => _textMessageHandler.ProcessMessageAsync(update.Message, cancellationToken),
            _ => Task.CompletedTask
        };
    }

    public static Task HandleErrorsAsync(Exception exception)
    {
        Log.Error(exception.Demystify(), nameof(HandleErrorsAsync));
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _client.ReceiveAsync((_, update, arg3) => HandleUpdatesAsync(update, arg3), (_, exception, _) => HandleErrorsAsync(exception),
                    StaticVariables.ReceiverOptions, cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Demystify(), nameof(ExecuteTask));
        }
    }
}