using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace WorkerService1.Common.Static;

internal static class StaticVariables
{
    public static string EnvironmentName { get; set; } = Environments.Development;

    public static ReceiverOptions ReceiverOptions { get; } = new()
    {
        AllowedUpdates = new[]
        {
            UpdateType.Message, UpdateType.ChatMember, UpdateType.MyChatMember, UpdateType.Unknown,
            UpdateType.ChatJoinRequest
        },
        ThrowPendingUpdates = true
    };
}