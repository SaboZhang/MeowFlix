using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;

[Table("Movies")]
public class MovieTable : BaseMediaTable
{
    public MovieTable(string title, string filePath, string dateTime, double fileSize, ServerType serverType, string channel, int? year)
        : base(title, filePath, dateTime, fileSize, serverType, channel, year)
    {

    }

    public MovieTable() { }

    public string? Description { get; set; }

    public string Originaltitle { get; set; }

    public string? SortTitle { get; set; }

    public string? Mpaa { get; set; }

    public string Imdbid { get; set; }

    public string Tmdbid { get; set; }

    public string ReleaseDate { get; set; }

    public int? Runtime { get; set; }

    public string Outline { get; set; }

    public string Country { get; set; }

    public string? Cover { get; set; }

    public string? Banner { get; set; }

    public string Logo { get; set; }

    public string? Set { get; set; }

    public string Fanart { get; set; }

    public DateTime CreaterTime { get; set; }

    public List<string> Genres { get; set; } = new List<string>();

    public FileInfoTable StreamDetails { get; set; }

    // public string StoargePath { get; set; }


}
