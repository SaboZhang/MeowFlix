using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;

[Table("UserTable")]
public class AuthTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool IsLocal { get; set; }

    public AuthTable(string username, string password, bool isLocal)
    {
        this.Username = username;
        this.Password = password;
        this.IsLocal = isLocal;
    }

    public AuthTable() { }
}
