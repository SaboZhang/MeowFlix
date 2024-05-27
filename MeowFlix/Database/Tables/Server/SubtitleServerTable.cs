using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;

[Table("SubtitleServer")]
public class SubtitleServerTable : BaseServerTable
{
    public SubtitleServerTable(string title, string filePath, bool isActive, ServerType serverType = ServerType.Subtitle)
        : base(title, filePath, isActive, serverType)
    {
    }
}
