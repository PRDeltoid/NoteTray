using System;
using NoteTrayLib.Models;
using NoteTrayLib.Services;
using NUnit.Framework;

namespace NoteTrayLibTests;

[TestFixture]
public class FileTrackerServiceTests
{
    private readonly IDatabaseService _database = new SQLiteDatabaseService("test_db");

    [Test]
    public void TrackFileTest()
    {
        FileTrackerService tracker = new FileTrackerService(_database);
        TrackedFileModel testFile = new TrackedFileModel()
        {
            FileName = "TestFile.txt",
            LastChanged = DateTime.Now,
            Path = "/path/to/file/"
        };
        tracker.TrackFile(testFile);
    }

    [Test]
    public void LargeTrackFileTest()
    {
        FileTrackerService tracker = new FileTrackerService(_database);
        for (int i = 0; i < 1000; i++)
        {
            TrackedFileModel testFile = new TrackedFileModel()
            {
                FileName = $"TestFile{i}.txt",
                LastChanged = DateTime.Today.AddMinutes(15),
                Path = "/path/to/file/"
            };
            // Exit early if the file is already tracked and up-to-date
            if (tracker.IsFileTracked(testFile.Path + testFile.FileName, testFile.LastChanged)) continue;
            
            tracker.TrackFile(testFile); 
        }
    }
    
    [Test]
    public void RemoveFileTest()
    {
        FileTrackerService tracker = new FileTrackerService(_database);
        Assert.IsTrue(tracker.RemoveFile("/path/to/file/TestFile1.txt")); 
    }
}