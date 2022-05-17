using NoteTrayLib.Models;
using NoteTrayLib.Utilities;

namespace NoteTrayLib.Services;

public class DirectoryManagerService
{
    private readonly string _rootPath;
    private string _directory;
    
    public bool IsRootDirectory => _directory == _rootPath;

    public DirectoryManagerService(UserPreferenceService userPreferences)
    {
        // Get the user's base note directory
        // If no preference exists, use the User Profile directory
        if (userPreferences.TryGetPreference("basePath", out _directory) == false)
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            userPreferences.SetPreference("basePath", userProfilePath);
            _directory = userProfilePath;
        }

        _rootPath = _directory;
    }
    
    public void SetCurrentDirectory(string path)
    {
        _directory = path;
    }

    public IEnumerable<NoteListItem> GetChildFiles()
    {
        return DirectoryUtilities.GetChildFiles(_directory);
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
}