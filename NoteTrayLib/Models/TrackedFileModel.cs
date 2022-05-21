namespace NoteTrayLib.Models;

public class TrackedFileModel
{
    public int ID { get; set; }
    public string Path { get; set; }
    public string FileName { get; set; }
    public string FullPath => System.IO.Path.Combine(Path, FileName);
    public DateTime LastChanged { get; set; }
}