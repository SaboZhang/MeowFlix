using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;
public class BaseServerTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Title { get; set; }
    public string FilePath { get; set; }
    public bool IsActive { get; set; }
    public ServerType ServerType { get; set; }

    public BaseServerTable(string title, string filePath, bool isActive, ServerType serverType = ServerType.Subtitle)
    {
        this.Title = title;
        this.FilePath = filePath;
        this.IsActive = isActive;
        this.ServerType = serverType;
    }
}
