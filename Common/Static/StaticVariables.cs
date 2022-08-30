using Telegram.Bot.Polling;

namespace SpeechToolsBot.Common.Static;

internal static class StaticVariables
{
    public const string BaseAudioPath = "Audio";
    public static readonly SecretManager SecretManager = new();
    public static string EnvironmentName { get; set; } = Environments.Development;

    public static ReceiverOptions ReceiverOptions { get; } = new() { ThrowPendingUpdates = true };
}