using NoteTrayLib.Models;
using Serilog;

namespace NoteTrayLib.Services;

public class FileTrackerService
{
    private const string TableName = "filetracking";
    private readonly IDatabaseService _database;

    public FileTrackerService(IDatabaseService database)
    {
        _database = database;
        CreatePreferenceTableIfAbsent();
    }

    public event EventHandler<TrackedFileModel> FileTracked;
    public event EventHandler<TrackedFileModel> FileRemoved;
    
    /// <summary>
    /// Start tracking a file.
    /// To speed up file tracking with large numbers of files, use IsFileTracked() before calling TrackFile(). If IsFileTracked() returns true, the TrackFile() call can be skipped
    /// </summary>
    /// <param name="file">The file to track</param>
    public void TrackFile(TrackedFileModel file)
    {
        var parameters = new
        {
            path = file.Path,
            filename = file.FileName,
            lastchanged = file.LastChanged
        };
        
        Log.Debug($@"INSERT INTO {TableName}(Path, FileName, LastChanged) 
                                                  VALUES({file.Path}, {file.FileName}, {file.LastChanged})");
        
        _database.ExecuteNonQuery($@"INSERT INTO {TableName}(Path, FileName, LastChanged) 
                                                  VALUES(@path, @filename, @lastchanged)", parameters); 
        FileTracked?.Invoke(this, file);
    }

    /// <summary>
    /// Determines if a file is correctly tracked in the database. This means the LastChanged time in the database matches the file, indicating it has not been altered
    /// </summary>
    /// <param name="file">The file to check</param>
    /// <returns>True if the file is correctly tracked. False if file is not currently tracked or LastChanged time is out-of-date</returns>
    public bool IsFileTracked(TrackedFileModel file)
    {
        var parameters = new
        {
            path = file.Path,
            filename = file.FileName,
        };

        IEnumerable<DateTime> result = _database.ExecuteQuery<DateTime>(
            @$"SELECT LastChanged FROM {TableName} WHERE FileName = @filename AND Path = @path", parameters);
        
        if (result.Any())
        {
            return (result.First() == file.LastChanged);
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// Remove a files tracking information
    /// </summary>
    /// <param name="file">The file to remove</param>
    /// <returns>True if a file was successfully removed. False otherwise. Failure may indicate that the file tracking information does not exist or the database is having other issues</returns>
    public bool RemoveFile(TrackedFileModel file)
    {
        var parameters = new
        {
            name = file.FileName,
            path = file.Path
        };
            
        Log.Debug($@"DELETE FROM {TableName} WHERE FileName = {file.FileName} AND Path={file.Path}");
        int rows = _database.ExecuteNonQuery($@"DELETE FROM {TableName} WHERE FileName = @name AND Path=path", parameters);
        if (rows > 0)
        {
            FileRemoved?.Invoke(this, file);
            return true;
        }
        else
        {
            return false;
        }
    }
    
    private void CreatePreferenceTableIfAbsent()
    {
        Log.Debug($@"CREATE TABLE IF NOT EXISTS {TableName}(
                                          ID INTEGER PRIMARY KEY,
                                          Path TEXT,
                                          FileName TEXT,
                                          LastChanged TEXT,
                                          UNIQUE(Path, FileName) ON CONFLICT REPLACE);
            )");
        _database.ExecuteNonQuery(
            @$"CREATE TABLE IF NOT EXISTS {TableName}(
                                          ID INTEGER PRIMARY KEY,
                                          Path TEXT,
                                          FileName TEXT,
                                          LastChanged TEXT,
                                          UNIQUE(Path, FileName) ON CONFLICT REPLACE);)");
    }
}