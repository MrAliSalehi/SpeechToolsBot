using System.Diagnostics;
using System.Text.Json;
using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.CognitiveServices.Speech;
using OneOf;
using OneOf.Types;
using SpeechToolsBot.ApiCalls;
using SpeechToolsBot.Common.Files;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpeechToolsBot.UpdateProcessors;

internal class AudioMessage
{
    private readonly ITelegramBotClient _client;
    private readonly SpeechToTextApi _speechToTextApi;

    public AudioMessage(ITelegramBotClient client, SpeechToTextApi speechToTextApi)
    {
        _client = client;
        _speechToTextApi = speechToTextApi;

        GlobalFFOptions.Configure(options =>
        {
            options.WorkingDirectory = @"D:\Projects\C#\TelegramProducts\BotApi\TextToSpeech\TextToSpeechBot\bin\Debug\net6.0\ffmpeg";
            options.BinaryFolder = @"D:\Projects\C#\TelegramProducts\BotApi\TextToSpeech\TextToSpeechBot\bin\Debug\net6.0\ffmpeg\bin\";
        });
    }

    public async Task ProcessAudioAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            await _client.SendTextMessageAsync(message.Chat.Id, "<i>Processing Your Audio...</i>", ParseMode.Html, cancellationToken: ct);

            OneOf<SpeechRecognitionResult, CancellationDetails, TrueFalseOrNull.Null> apiResponse;

            var downloadedFilePath = await GetFileAndDownloadItAsync(message, ct);

            if (downloadedFilePath.EndsWith("wav"))
            {
                apiResponse = await _speechToTextApi.ProcessAudioAsync(downloadedFilePath);
            }
            else
            {
                var newFilePathToSaveConversion = await ConvertTheMediaAndSaveItAsync(downloadedFilePath);

                apiResponse = await _speechToTextApi.ProcessAudioAsync(newFilePathToSaveConversion);
            }

            var result = "Result Of Process:\n" +
                         apiResponse.Match<string>(
                             t0 => $"Status:{t0.Reason}\n" +
                                   $"Duration:{t0.Duration:g}\n" +
                                   $"Text:\n<i>[{t0.Text}]</i>",
                             t1 => "The Process Failed:(\n" +
                                   $"Reason:{t1.Reason}\n" +
                                   $"ErrorCode:{t1.ErrorCode}\n" +
                                   $"{t1.ErrorDetails}",
                             _ => throw new InvalidOperationException());

            await _client.SendTextMessageAsync(message.Chat.Id, result, ParseMode.Html, cancellationToken: ct);
            Console.WriteLine(JsonSerializer.Serialize(apiResponse.AsT0.Best()));
        }
        catch (Exception e)
        {
            if (e is not InvalidOperationException)
                Log.Error(e.Demystify(), nameof(ProcessAudioAsync));
            var msgToSend = "Where was some issue with bot:(.\nplease try again later";
            if (e is InvalidDataException)
            {
                msgToSend = "Cant Convert Your Audio Format:(\n Please Send a .Wav or a valid format.";
            }
            await _client.SendTextMessageAsync(message.Chat.Id, msgToSend, cancellationToken: ct);
        }
    }

    private async ValueTask<string> GetFileAndDownloadItAsync(Message message, CancellationToken ct = default)
    {
        var fileId = GetFileId(message);
        var fileInfo = await _client.GetFileAsync(fileId, ct);

        if (string.IsNullOrEmpty(fileInfo.FilePath))
            throw new InvalidOperationException();


        var fileNameWithoutPath = Path.GetFileName(fileInfo.FilePath);

        var filepath = FileCreator.CreateFile()
            .Folder(Environment.CurrentDirectory)
            .Folder(StaticVariables.BaseAudioPath)
            .FolderWithCurrentYear()
            .FolderWithCurrentMonth()
            .FolderWithCurrentDay()
            .FullNameWithFormat(fileNameWithoutPath)
            .BuildPathAndFile();

        await using var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
        await _client.DownloadFileAsync(fileInfo.FilePath, fileStream, ct);

        return filepath;
    }

    private static async Task<string> ConvertTheMediaAndSaveItAsync(string downloadedFilePath)
    {
        var newFilePathToSaveConversion = FileCreator
            .CreateFile()
            .Folder(Environment.CurrentDirectory)
            .Folder(StaticVariables.BaseAudioPath)
            .FolderWithCurrentYear()
            .FolderWithCurrentMonth()
            .FolderWithCurrentDay()
            .RandomFileWithFormat("wav")
            .BuildPathAndFile();

        await using var destinationStream = new FileStream(newFilePathToSaveConversion, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        var convertedSuccessfully = await FFMpegArguments
            .FromFileInput(downloadedFilePath)
            .OutputToPipe(new StreamPipeSink(destinationStream), options => options.ForceFormat("wav"))
            .ProcessAsynchronously();

        if (!convertedSuccessfully)
            throw new InvalidDataException();

        return newFilePathToSaveConversion;
    }

    private static string GetFileId(Message message)
    {
        string fileId;

        if (message.Audio is not null)
        {
            fileId = message.Audio.FileId;
        }
        else if (message.Voice is not null)
        {
            fileId = message.Voice.FileId;
        }
        else if (message.Document is not null)
        {
            fileId = message.Document.FileId;
        }
        else
        {
            throw new InvalidOperationException();
        }
        return fileId;
    }
}