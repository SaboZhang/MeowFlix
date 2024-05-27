using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowFlix.Naming;

public class SeriesInfo
{
    public SeriesInfo(string path)
    {
        Path = path;
    }

    public string Path { get; set; }

    public string? Name { get; set; }
}
