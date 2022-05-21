namespace NoteTrayLib.Models;

public class SearchResultEventArgs
{
    public string Query { get; }
    public IEnumerable<SearchResultModel> Results { get; }

    public SearchResultEventArgs(string query, IEnumerable<SearchResultModel> results)
    {
        Query = query;
        Results = results;
    }
}