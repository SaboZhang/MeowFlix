using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowFlix.Database.Tables;

[Table("StorageTable")]
public class StorageTable : BaseServerTable
{

    public StorageTable(string title, string filePath, bool isActive, ServerType serverType, bool isPrivate)
        : base(title, filePath, isActive, serverType)
    {
        IsPrivate = isPrivate;
    }

    public bool IsPrivate { get; set; }

}
