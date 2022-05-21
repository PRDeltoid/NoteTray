using NoteTrayLib.Models;

namespace NoteTrayLib.Utilities;

public static class FileTrackerUtilities
{
    public static TrackedFileModel TrackedFileModelFromPath(string fullPath)
    {
        FileInfo fileInfo = new FileInfo(fullPath);
        return new TrackedFileModel()
        {
            FileName = Path.GetFileName(fullPath),
            Path = Path.GetDirectoryName(fullPath),
            LastChanged = fileInfo.LastWriteTime
        };
    }
}