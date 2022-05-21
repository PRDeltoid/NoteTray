namespace NoteTrayLib.Services;

public interface IFullTextSearchService
{
    void BuildIndex(string searchPath, bool flushIndex);
    void IndexFile(string filePath);
    void RemoveFile(string filePath);
    void Search(string text);
}