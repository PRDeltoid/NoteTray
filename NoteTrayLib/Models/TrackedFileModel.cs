namespace NoteTrayLib.Models;

public class TrackedFileModel
{
    public int ID { get; set; }
    public string Path { get; set; }
    public string FileName { get; set; }
    public DateTime LastChanged { get; set; }
}