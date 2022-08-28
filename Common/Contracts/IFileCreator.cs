namespace SpeechToolsBot.Common.Contracts;

public interface IFileCreator
{
    IFileCreator FileNameAndFormat(string name, string format);
    string BuildPathAndFile();
    IFileCreator Folder(string folderName);
    IFileCreator FolderWithCurrentYear();
    IFileCreator FolderWithCurrentMonth();
    IFileCreator FolderWithCurrentDay();
    IFileCreator RandomFileWithFormat(string format);
    IFileCreator FullNameWithFormat(string fileName);
}