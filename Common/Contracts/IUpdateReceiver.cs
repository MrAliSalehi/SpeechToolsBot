﻿using Telegram.Bot;
using Telegram.Bot.Types;

namespace SpeechToolsBot.Common.Contracts;

public interface IUpdateReceiver
{
    static Task HandleUpdatesAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    static Task HandleErrorsAsync(Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;
}