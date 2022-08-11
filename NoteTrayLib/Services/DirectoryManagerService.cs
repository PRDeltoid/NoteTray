using System.ComponentModel;
using NoteTrayLib.Models;
using NoteTrayLib.Utilities;

namespace NoteTrayLib.Services;

public class DirectoryManagerService
{
    private readonly UserPreferenceService _userPrefs;
    private readonly string _rootPath;
    private string _directory;
    private string _fileFilter;

    public bool IsRootDirectory => _directory == _rootPath;

    public string CurrentDirectory => _directory;

    public DirectoryManagerService(UserPreferenceService userPreferences)
    {
        _userPrefs = userPreferences;
        // Get the user's base note directory
        // If no preference exists, use the User Profile directory
        if (userPreferences.BasePath == null)
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            userPreferences.BasePath = userProfilePath;
            _directory = userProfilePath;
        }

        _directory = userPreferences.BasePath;

        _fileFilter = _userPrefs.NoteFileFilter;
        _userPrefs.PropertyChanged += OnFileFilterChanged;

        _rootPath = _directory;
    }

    private void OnFileFilterChanged(object sender, PropertyChangedEventArgs e)
    {
        _fileFilter = _userPrefs.NoteFileFilter; 
    }

    public void SetCurrentDirectory(string path)
    {
        _directory = path;
    }

    public IEnumerable<NoteListItem> GetChildFiles()
    {
        return DirectoryUtilities.GetChildFiles(_directory, _fileFilter);
    }

    public IEnumerable<NoteListItem> GetChildDirectories()
    {
        return DirectoryUtilities.GetChildDirectories(_directory);
    }

    public NoteListItem GetParent()
    {
        return DirectoryUtilities.GetParentDirectory(_directory);
    }

    public bool TryMoveUp()
    {
        // Exit with failure if we are already at the "root"
        if (IsRootDirectory) return false;
        // Exit with failure if there no parent directory exists (we are at the filesystem's root)
        NoteListItem parentDir = DirectoryUtilities.GetParentDirectory(_directory);
        if (parentDir == null) return false;

        // Move up to the parent directory and return success
        _directory = parentDir.FullPath;
        return true;
    }

    public void DoToEachFile(string dir, bool recursive, Action<NoteListItem> action)
    {
        foreach (NoteListItem item in DirectoryUtilities.GetChildFiles(dir, _fileFilter))
        {
            action.Invoke(item);
        }

        if (recursive)
        {
            foreach (NoteListItem item in DirectoryUtilities.GetChildDirectories(dir))
            {
                DoToEachFile(item.FullPath, true, action);
            } 
        }
    }
}