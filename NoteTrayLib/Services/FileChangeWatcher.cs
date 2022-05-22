using NoteTrayLib.Models;
using NoteTrayLib.Utilities;
using Serilog;

namespace NoteTrayLib.Services;

public class FileChangeWatcher
{
    private readonly FileTrackerService _trackerService;
    private readonly FileSystemWatcher _watcher;

    public FileChangeWatcher(FileTrackerService trackerService, UserPreferenceService userPrefs, bool includeSubdirectories)
    {
        _trackerService = trackerService;
        
        // TODO: Handle this better? Is there a safe default?
        string basePath = userPrefs.BasePath ?? throw new Exception("No basePath found for FileChangeWatcher to watch"); 
        
        _watcher = new FileSystemWatcher(basePath);

        _watcher.NotifyFilter = NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastWrite;

        _watcher.Changed += OnChanged;
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnError;

        _watcher.Filter = "*";
        _watcher.IncludeSubdirectories = includeSubdirectories;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Log.Error("FileTrackerService error occurred: {exception}", e.GetException());
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (IsFileHidden(e.FullPath)) return;
        
        Log.Debug("FileTrackerService OnRenamed event occurred: {path}", e.FullPath);
        _trackerService.RemoveFile(e.OldFullPath);
        TrackedFileModel file = FileTrackerUtilities.TrackedFileModelFromPath(e.FullPath);
        _trackerService.TrackFile(file);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Log.Debug("FileTrackerService OnDeleted event occurred: {path}", e.FullPath);
        _trackerService.RemoveFile(e.FullPath);
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        // OnCreated is always followed by an OnChanged call so we don't have to do anything here, it'll be caught later
        Log.Debug("FileTrackerService OnCreated event occurred: {path}", e.FullPath);
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (IsFileHidden(e.FullPath)) return;
        
        Log.Debug("FileTrackerService OnChanged event occurred: {path}", e.FullPath);
        TrackedFileModel file = FileTrackerUtilities.TrackedFileModelFromPath(e.FullPath);
        _trackerService.TrackFile(file);
    }
    
    private static bool IsFileHidden(string fullPath)
    {
        return File.GetAttributes(fullPath).HasFlag(FileAttributes.Hidden);;
    }
}