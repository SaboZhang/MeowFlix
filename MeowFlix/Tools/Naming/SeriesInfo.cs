namespace MeowFlix.Naming;

public class SeriesInfo
{
    public SeriesInfo(string path)
    {
        Path = path;
    }

    public string Path { get; set; }

    public string? Name { get; set; }
}
