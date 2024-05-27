using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowFlix.Naming;

public class SeasonPathParserResult
{
    public int? SeasonNumber { get; set; }

    public bool Success { get; set; }

    public bool IsSeasonFolder { get; set; }
}
