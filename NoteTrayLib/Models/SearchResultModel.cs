namespace NoteTrayLib.Models;

public class SearchResultModel
{
    public string FullPath { get; internal set; }
    public string Name { get; internal set; }
    public float SearchScore { get; internal set; }
}