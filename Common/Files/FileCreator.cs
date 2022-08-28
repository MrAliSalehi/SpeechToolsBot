namespace WorkerService1.Common.Files;

internal class FileCreator : IFileCreator
{
    private FileCreator()
    {
    }

    internal static FileCreator CreateFile() => new();

    private readonly List<string> _folders = new();
    private string _fileName = string.Empty;

    public IFileCreator Folder(string folderName)
    {
        _folders.Add(folderName);
        return this;
    }

    public IFileCreator FolderWithCurrentYear()
    {
        _folders.Add($"Y_{DateTime.Now.Year}");
        return this;
    }

    public IFileCreator FolderWithCurrentMonth()
    {
        _folders.Add($"M_{DateTime.Now.Month}");
        return this;
    }

    public IFileCreator FolderWithCurrentDay()
    {
        _folders.Add($"D_{DateTime.Now.Day}");
        return this;
    }

    public IFileCreator RandomFileWithFormat(string format)
    {
        _fileName = Path.GetRandomFileName() + $".{format}";
        return this;
    }

    public IFileCreator FileNameAndFormat(string name, string format)
    {
        _fileName = name + $".{format}";
        return this;
    }

    public string BuildPathAndFile()
    {
        var path = Path.Combine(_folders.ToArray());

        if (!Directory.Exists(path))
            path = Directory.CreateDirectory(path).FullName;

        var combineFileAndPath = Path.Combine(path, _fileName);
        if (File.Exists(combineFileAndPath))
            return combineFileAndPath;

        using var createdStream = File.Create(combineFileAndPath);
        createdStream.Dispose();
        return combineFileAndPath;
    }
}