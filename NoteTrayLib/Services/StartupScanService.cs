using NoteTrayLib.Models;
using NoteTrayLib.Utilities;
using Serilog;

namespace NoteTrayLib.Services;

public class StartupScanService
{
    private readonly FileTrackerService _trackerService;
    private readonly DirectoryManagerService _directoryManagerService;
    private readonly string _basePath;

    public StartupScanService(FileTrackerService trackerService, DirectoryManagerService directoryManagerService, UserPreferenceService userPrefs)
    {
        _trackerService = trackerService;
        _directoryManagerService = directoryManagerService;

        // TODO: Handle this better? Is there a safe default?
        _basePath = userPrefs.BasePath ?? throw new Exception("No basePath found for FileChangeWatcher to watch"); 
    }
    
    public void Scan()
    {
        // Compare the filesystem state at basePath to the filetrackersystem state
        // anything missing from filesystem but in filetracker has been deleted
        // anything in filesystem not in filetracker has been added while notetray was not open
        // if it's in both, update to filetrackersystem may be needed
        
        int scannedCount = 0;
        int addedCount = 0;
        int removedCount = 0;
        Dictionary<string, TrackedFileModel> trackedFiles = _trackerService.TrackedFiles.ToDictionary(x => x.FullPath);
        _directoryManagerService.DoToEachFile(_basePath, true, (item =>
        {
            trackedFiles.Remove(item.FullPath);
            scannedCount++;
            // If the file is already correctly tracked, return early
            if (_trackerService.IsFileTracked(item.FullPath, File.GetLastWriteTime(item.FullPath))) return;

            addedCount++;
            // Otherwise, track the file
            _trackerService.TrackFile(FileTrackerUtilities.TrackedFileModelFromNoteListItem(item));
        }));

        // trackedFiles now contains only files we didn't find in the directory, so we should remove them
        foreach (TrackedFileModel removedFile in trackedFiles.Values)
        {
            removedCount++;
            _trackerService.RemoveFile(removedFile.FullPath);
        }
        
        Log.Debug("Startup Scan has found {scannedCount} documents, added/updated {addedCount} and removed {removedCount} files that no longer exist", scannedCount, addedCount, removedCount);
    }
}