using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;

[Table("Subtitle")]
public class SubtitleTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Language { get; set; }

    public string FilePath { get; set; }

    public string Codec { get; set; }

    public DateTime CreaterTime { get; set; }
}
