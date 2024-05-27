using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MeowFlix.Naming
{
    public readonly struct CleanDateTimeResult
    {
        public CleanDateTimeResult(string name, int? year = null)
        {
            Name = name;
            Year = year;
        }

        public string Name { get; }

        public int? Year { get; }
    }
}
