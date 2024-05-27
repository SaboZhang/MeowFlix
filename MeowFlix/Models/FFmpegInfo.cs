using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowFlix.Models
{
    public class FFmpegInfo
    {
        public string? Container { get; set; }
        public long Duration { get; set; }

        public string AudioCodec { get; set; }

        public string VideoCodec { get; set; }

        public List<string>? SubtitleLanguage { get; set; }

        public List<string>? AudioLanguage { get; set; }

        public string Quality { get; set; }

        public int ChannelsCount { get; set; }

    }
}
