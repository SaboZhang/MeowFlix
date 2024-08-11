using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;

[Table("MediaServer")]
public class MediaServerTable : BaseServerTable
{
    public MediaServerTable(string title, string filePath, bool isActive, ServerType serverType)
        : base(title, filePath, isActive, serverType)
    {
    }

}
