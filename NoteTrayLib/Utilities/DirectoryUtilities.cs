using NoteTrayLib.Models;

namespace NoteTrayLib.Utilities;

public static class DirectoryUtilities
{
    public static IEnumerable<NoteListItem> GetChildDirectories(string basePath)
    {
        return Directory.GetDirectories(basePath).Where(x =>
        {
            // Hide hidden directories
            FileInfo fileInfo = new FileInfo(x);
            return fileInfo.Attributes.HasFlag(FileAttributes.Hidden) == false;
        }).Select(x => new NoteListItem() {FullPath = x, Name = Path.GetFileName(x), IsDirectory = true});
    }

    public static IEnumerable<NoteListItem> GetChildFiles(string basePath)
    {
        return Directory.GetFiles(basePath).Where(x =>
        {
            // Hide hidden files
            FileInfo fileInfo = new FileInfo(x);
            return fileInfo.Attributes.HasFlag(FileAttributes.Hidden) == false;
        }).Select(x => new NoteListItem() {FullPath = x, Name = Path.GetFileName(x), IsDirectory = false});
    }

    public static NoteListItem GetParentDirectory(string basePath)
    {
        DirectoryInfo? parent = Directory.GetParent(basePath);
        if (parent != null)
        {
            return new NoteListItem() { FullPath = parent.FullName, Name = parent.Name, IsDirectory = true };
        }
        else
        {
            return null;
        }
    }
}