namespace NoteTrayLib;

public class DirectoryService
{
    public IEnumerable<string> GetChildDirectories(string basePath)
    {
        return Directory.GetDirectories(basePath).Where(x =>
        {
            var fileInfo = new FileInfo(x);
            return fileInfo.Attributes.HasFlag(FileAttributes.Hidden) == false;
        }).Select(Path.GetFileName).Where(x => x != null)!;
    }

    public IEnumerable<string> GetChildFiles(string basePath)
    {
        return Directory.GetFiles(basePath).Where(x =>
        {
            var fileInfo = new FileInfo(x);
            return fileInfo.Attributes.HasFlag(FileAttributes.Hidden) == false;
        }).Select(Path.GetFileName).Where(x => x != null)!;
    }
}