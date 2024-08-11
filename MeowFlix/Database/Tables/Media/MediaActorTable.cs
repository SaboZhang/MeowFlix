using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;

[Table("MediaActorTable")]
public class MediaActorTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Actor { get; set; }

    public string Role { get; set; }

    public string Thumb { get; set; }

    public string Profile { get; set; }
}
