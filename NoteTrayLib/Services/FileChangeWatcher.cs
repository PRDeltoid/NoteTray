using System.ComponentModel;
using NoteTrayLib.Models;
using NoteTrayLib.Utilities;
using Serilog;

namespace NoteTrayLib.Services;

public class FileChangeWatcher
{
    private readonly FileTrackerService _trackerService;
    private readonly UserPreferenceService _userPrefs;
    private readonly bool _includeSubdirectories;
    private readonly ISet<FileSystemWatcher> _watchers;

    public event EventHandler<TrackedFileModel> FileAdded;
    public event EventHandler<TrackedFileModel> FileRemoved;

    public FileChangeWatcher(FileTrackerService trackerService, UserPreferenceService userPrefs, bool includeSubdirectories)
    {
        _trackerService = trackerService;
        _userPrefs = userPrefs;
        _includeSubdirectories = includeSubdirectories;
        _watchers = new HashSet<FileSystemWatcher>();
        
        string basePath = userPrefs.BasePath ?? throw new Exception("No basePath found for FileChangeWatcher to watch");

        // We need one watcher per extension in the user's file filter because of filesystemwatcher filter limitations
        CreateWatchers(basePath, includeSubdirectories, _userPrefs.NoteFileFilter);
        // Update watchers if the NoteFileFilter property changes
        userPrefs.PropertyChanged += OnUserFilterChange;
    }

    private void CreateWatchers(string basePath, bool includeSubdirectories, string filters = "")
    {
        foreach (string filter in filters.Split("|"))
        {
            CreateWatcher(basePath, includeSubdirectories, filter);
        }
    }

    private void OnUserFilterChange(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "noteFileFilter") return;
        ClearWatchers();
        CreateWatchers(_userPrefs.BasePath, _includeSubdirectories, _userPrefs.NoteFileFilter);
    }

    private void CreateWatcher(string path, bool includeSubdirectories, string filter = "")
    {
        FileSystemWatcher watcher = new FileSystemWatcher(path);

        watcher.NotifyFilter = NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastWrite;

        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;
        watcher.Error += OnError;

        if (filter != "")
        {
            watcher.Filter = filter;
        }
        
        watcher.IncludeSubdirectories = includeSubdirectories;
        watcher.EnableRaisingEvents = true;
        _watchers.Add(watcher);
    }

    private void ClearWatchers()
    {
        foreach (FileSystemWatcher fileSystemWatcher in _watchers)
        {
            fileSystemWatcher.EnableRaisingEvents = false;
            fileSystemWatcher.Dispose();
        }

        _watchers.Clear();
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
        FileRemoved?.Invoke(this, FileTrackerUtilities.TrackedFileModelFromPath(e.OldFullPath));
        TrackedFileModel file = FileTrackerUtilities.TrackedFileModelFromPath(e.FullPath);
        _trackerService.TrackFile(file);
        FileAdded?.Invoke(this, file);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Log.Debug("FileTrackerService OnDeleted event occurred: {path}", e.FullPath);
        _trackerService.RemoveFile(e.FullPath);
        FileRemoved?.Invoke(this, FileTrackerUtilities.TrackedFileModelFromPath(e.FullPath));
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        // OnCreated is always followed by an OnChanged call so we don't have to do anything here, it'll be caught later
        if (IsFileHidden(e.FullPath)) return;
        
        Log.Debug("FileTrackerService OnCreated event occurred: {path}", e.FullPath);
        TrackedFileModel file = FileTrackerUtilities.TrackedFileModelFromPath(e.FullPath);
        _trackerService.TrackFile(file);
        FileAdded?.Invoke(this, file);
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        Log.Debug("FileTrackerService OnChanged event occurred: {path}", e.FullPath);
    }
    
    private static bool IsFileHidden(string fullPath)
    {
        return File.GetAttributes(fullPath).HasFlag(FileAttributes.Hidden);;
    }
}