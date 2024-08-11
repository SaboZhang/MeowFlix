using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeowFlix.Database.Tables;

[Table("FileInfoTable")]
public class FileInfoTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }

    public string VideoCodec { get; set; }

    public string VideoResolution { get; set; }

    public string MiCodec { get; set; }

    public string VideoWidth { get; set; }

    public string VideoHeight { get; set; }

    public string Aspect { get; set; }

    public string VideoDuration { get; set; }

    public string AspectRatio { get; set; }

    public string VideoFrameRate { get; set; }

    public string VideoBitrate { get; set; }

    public string AudioCodec { get; set; }

    public string AudioBitrate { get; set; }

    public string AudioMiCodec { get; set; }

    public string Language { get; set; }

    public string AudioChannels { get; set; }

    public string SampLingrate { get; set; }

    public List<SubtitleTable> Subtitles { get; set; }


    public FileInfoTable()
    {

    }
}
