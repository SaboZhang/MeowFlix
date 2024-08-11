using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;

[Table("ChannelTable")]
public class ChannelTable : BaseServerTable
{
    public ChannelTable(string title, string filePath, bool isActive, ServerType serverType = ServerType.Subtitle)
        : base(title, filePath, isActive, serverType)
    {
    }
}
