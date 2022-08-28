using System.Diagnostics;
using Microsoft.CognitiveServices.Speech;

namespace SpeechToolsBot.ApiCalls;

internal class TextToSpeechApi
{
    private readonly SpeechConfig _speechConfig;

    public TextToSpeechApi(SpeechConfig speechConfig)
    {
        _speechConfig = speechConfig;
    }

    internal async Task<SpeechSynthesisResult?> GetAudioAsync(string textToConvert)
    {
        try
        {
            using var synthesizer = new SpeechSynthesizer(_speechConfig);

            return await synthesizer.SpeakTextAsync(textToConvert);
        }
        catch (Exception exception)
        {
            Log.Error(exception.Demystify(), nameof(GetAudioAsync));
            return null;
        }
    }
}