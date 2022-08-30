namespace SpeechToolsBot.Common.Exceptions;

internal class InvalidCommandException : Exception
{
    public InvalidCommandException(string command) : base(command)
    {
    }
}