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
    public event EventHandler<string> FileRemoved;
    
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
    /// Determines if a file is correctly tracked. This means the LastChanged time in the database matches the file, indicating it has not been altered
    /// </summary>
    /// <param name="fullPath">The fully qualified path to the file (including the file name)</param>
    /// <param name="currentLastChanged">The last date changed of the file we are checking</param>
    /// <returns>True if the file is correctly tracked. False if file is not currently tracked or LastChanged time is out-of-date</returns>
    public bool IsFileTracked(string fullPath, DateTime currentLastChanged)
    {
        string path = Path.GetDirectoryName(fullPath);
        string filename = Path.GetFileName(fullPath);
        
        IEnumerable<DateTime> result = _database.ExecuteQuery<DateTime>(
            @$"SELECT LastChanged FROM {TableName} WHERE FileName = @filename AND Path = @path", new { filename, path });
        
        if (result.Any())
        {
            return (result.First() == currentLastChanged);
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// Remove a files tracking information
    /// </summary>
    /// <param name="fullPath">The fully qualified path to the file (including the file name)</param>
    /// <returns>True if a file was successfully removed. False otherwise. Failure may indicate that the file tracking information does not exist or the database is having other issues</returns>
    public bool RemoveFile(string fullPath)
    {
        string path = Path.GetDirectoryName(fullPath);
        string filename = Path.GetFileName(fullPath);
        
        Log.Debug($@"DELETE FROM {TableName} WHERE FileName={filename} AND Path={path}");
        
        int rows = _database.ExecuteNonQuery($@"DELETE FROM {TableName} WHERE FileName=@filename AND Path=@path", new { filename, path });
        if (rows > 0)
        {
            FileRemoved?.Invoke(this, fullPath);
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
                                          UNIQUE(Path, FileName) ON CONFLICT REPLACE");
        _database.ExecuteNonQuery(
            @$"CREATE TABLE IF NOT EXISTS {TableName}(
                                          ID INTEGER PRIMARY KEY,
                                          Path TEXT,
                                          FileName TEXT,
                                          LastChanged TEXT,
                                          UNIQUE(Path, FileName) ON CONFLICT REPLACE");
    }
}