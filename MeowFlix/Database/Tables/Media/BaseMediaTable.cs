using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;
public class BaseMediaTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Title { get; set; }
    public string FilePath { get; set; }
    public string DateTime { get; set; }
    public double FileSize { get; set; }
    public string GroupKey { get; set; }
    public ServerType ServerType { get; set; }
    public string TitleAndServer => $"{Title}###{FilePath}";
    public string Poster { get; set; }
    public string Likes { get; set; }
    public string Rating { get; set; }
    public string Channel { get; set; }

    public int? Year { get; set; }

    public List<MediaActorTable> Actors { get; set; }

    public BaseMediaTable()
    {
        
    }

    public BaseMediaTable(string title, string filePath, string dateTime, double fileSize, ServerType serverType, string channel, int? year)
    {
        this.Title = title;
        this.FilePath = filePath;
        this.DateTime = dateTime;
        this.FileSize = fileSize;
        this.ServerType = serverType;
        this.Channel = channel;
        this.Year = year;
    }
}
