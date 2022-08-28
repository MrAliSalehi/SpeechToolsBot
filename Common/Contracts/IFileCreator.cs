namespace WorkerService1.Common.Files;

public interface IFileCreator
{
    public IFileCreator FileNameAndFormat(string name, string format);
    public string BuildPathAndFile();
}