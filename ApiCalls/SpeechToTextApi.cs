using System.Diagnostics;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using OneOf;
using OneOf.Types;

namespace SpeechToolsBot.ApiCalls;

internal class SpeechToTextApi
{
    private static readonly TrueFalseOrNull.Null DefaultNull = new();
    private readonly SpeechConfig _speechConfig;
    public SpeechToTextApi(SpeechConfig speechConfig) { _speechConfig = speechConfig; }

    public async ValueTask<OneOf<SpeechRecognitionResult, CancellationDetails, TrueFalseOrNull.Null>> ProcessAudioAsync(string path)
    {
        try
        {
            using var audioConfig = AudioConfig.FromWavFileInput(path);

            using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);

            var speechRecognitionResult = await recognizer.RecognizeOnceAsync();
            if (speechRecognitionResult is null)
                return DefaultNull;


            if (speechRecognitionResult.Reason != ResultReason.Canceled)
                return speechRecognitionResult;

            var cancellationDetails = CancellationDetails.FromResult(speechRecognitionResult);

            return cancellationDetails;
        }
        catch (Exception e)
        {
            Log.Error(e.Demystify(), nameof(ProcessAudioAsync));
            return DefaultNull;
        }
    }
}