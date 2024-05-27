using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowFlix.Models
{
    public class VideoInfo
    {
        public string Path { get; set; }

        public string Name { get; set; }

        public string FileName { get; set; }

        public int? Year { get; set; }

        public long Size { get; set; }

        public FFmpegInfo? FfMpegInfo { get; set; }

        public int? Season { get; set; }

        public int? Episode { get; set; }



        public VideoInfo(string path, string name, string fileName, int? year, long size, int? season, int? episode, FFmpegInfo ffMpegInfo)
        {
            Path = path;
            Name = name;
            FileName = fileName;
            Year = year;
            Size = size;
            FfMpegInfo = ffMpegInfo;
            Season = season;
            Episode = episode;
        }
    }
}
