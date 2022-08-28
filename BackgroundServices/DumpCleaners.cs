using System.Diagnostics;
using System.Reactive.Linq;
using NodaTime;

namespace SpeechToolsBot.BackgroundServices;

internal static class DumpCleaners
{
    private readonly static List<string> FilePathList = new();
    private static DateTimeOffset _offsetToDelete = DateTimeOffset.Now;
    private static TimeSpan _timeToDelete = TimeSpan.Zero;

    internal static void StartTheCleaner()
    {
        Observable.Timer(_offsetToDelete, _timeToDelete).Subscribe(_ =>
        {
            try
            {
                DeleteAudios();
            }
            catch (Exception e)
            {
                Log.Error(e.Demystify(), nameof(StartTheCleaner));
            }
        });
    }

    private static void DeleteAudios()
    {
        foreach (var path in Directory.GetFiles(StaticVariables.BaseAudioPath, "*", SearchOption.AllDirectories))
        {
            var fileInfo = new FileInfo(path);

            var between = Period.Between(LocalDateTime.FromDateTime(fileInfo.CreationTime),
                LocalDateTime.FromDateTime(DateTime.Now), PeriodUnits.DateAndTime);
            if (between.Days > 0 || between.Hours > 1 || between.Minutes > 20)
            {
                FilePathList.Add(fileInfo.FullName);
            }
        }

        FilePathList.ForEach(filePath => File.Delete(filePath));
        FilePathList.Clear();
    }

    internal static void CreateImageFolderIfNeeded()
    {
        try
        {
            if (!Directory.Exists(StaticVariables.BaseAudioPath))
            {
                Directory.CreateDirectory(StaticVariables.BaseAudioPath);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Demystify(), nameof(CreateImageFolderIfNeeded));
        }
    }

    internal static void SetDeletionTimeBasedOnEnvironments()
    {
        switch (StaticVariables.EnvironmentName)
        {
            case nameof(Environments.Development):
                _timeToDelete = TimeSpan.FromMinutes(2);
                _offsetToDelete = DateTimeOffset.Now.AddSeconds(20);
                break;
            case nameof(Environments.Production):
                _timeToDelete = TimeSpan.FromMinutes(2);
                _offsetToDelete = DateTimeOffset.Now.AddSeconds(20);
                break;
        }
    }
}