using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoteTrayLib;

public class DirectoryService
{
    public static IEnumerable<string> GetChildDirectories(string basePath)
    {
        return Directory.GetDirectories(basePath).Where(x =>
        {
            FileInfo fileInfo = new FileInfo(x);
            return fileInfo.Attributes.HasFlag(FileAttributes.Hidden) == false;
        }).Select(Path.GetFileName).Where(x => x != null)!;
    }

    public static IEnumerable<string> GetChildFiles(string basePath)
    {
        return Directory.GetFiles(basePath).Where(x =>
        {
            FileInfo fileInfo = new FileInfo(x);
            return fileInfo.Attributes.HasFlag(FileAttributes.Hidden) == false;
        }).Select(Path.GetFileName).Where(x => x != null)!;
    }
}