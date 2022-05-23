using System.Text.RegularExpressions;
using NoteTrayLib.Models;

namespace NoteTrayLib.Utilities;

public static class DirectoryUtilities
{
    public static IEnumerable<NoteListItem> GetChildDirectories(string basePath)
    {
        return Directory.EnumerateDirectories(basePath).Where(x =>
        {
            // Hide hidden directories
            FileInfo fileInfo = new FileInfo(x);
            return fileInfo.Attributes.HasFlag(FileAttributes.Hidden) == false;
        }).Select(x => new NoteListItem() {FullPath = x, Name = Path.GetFileName(x), IsDirectory = true});
    }

    public static IEnumerable<NoteListItem> GetChildFiles(string basePath, string filter =  null)
    {
        Regex searchPattern;
        if (filter != null)
        {
            searchPattern = new Regex( filter.Replace('*', '\\'), RegexOptions.IgnoreCase); 
        }
        else
        {
            // If no filter is present, just use a wildcard to get all results
            searchPattern = new Regex("*", RegexOptions.IgnoreCase); 
        }
        
        return Directory.EnumerateFiles(basePath).Where(file =>
        {
            // Hide hidden files
            FileInfo fileInfo = new FileInfo(file);
            return fileInfo.Attributes.HasFlag(FileAttributes.Hidden) == false;
        }).Where(file => searchPattern.IsMatch(Path.GetExtension(file)))
          .Select(x => new NoteListItem() {FullPath = x, Name = Path.GetFileName(x), IsDirectory = false});
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