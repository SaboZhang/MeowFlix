using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;

[Table("Anime")]
public class AnimeTable : BaseMediaTable
{
    public AnimeTable(string title, string filePath, string dateTime, double fileSize, ServerType serverType, string channel, int? year)
        : base(title, filePath, dateTime, fileSize, serverType, channel, year)
    {
    }
}
