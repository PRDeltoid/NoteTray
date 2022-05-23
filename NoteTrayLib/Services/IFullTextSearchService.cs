using NoteTrayLib.Models;

namespace NoteTrayLib.Services;

public interface IFullTextSearchService
{
    event EventHandler IndexingStart;
    event EventHandler IndexingComplete;
    
    Task BuildIndex(string searchPath, bool flushIndex);
    void IndexFile(string filePath);
    void RemoveFile(string filePath);
    IEnumerable<SearchResultModel> Search(string text);
    event EventHandler<SearchResultEventArgs> SearchResultsAvailable;
}