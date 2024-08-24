using FFMpegCore;

namespace MeowFlix.Models
{
    public class MediaMeta : IMediaAnalysis
    {
        public TimeSpan Duration { get; set; }

        public MediaFormat Format { get; set; }

        public AudioStream PrimaryAudioStream { get; set; }

        public VideoStream PrimaryVideoStream { get; set; }

        public SubtitleStream PrimarySubtitleStream { get; set; }

        public List<VideoStream> VideoStreams { get; set; }

        public List<AudioStream> AudioStreams { get; set; }

        public List<SubtitleStream> SubtitleStreams { get; set; }

        IReadOnlyList<string> IMediaAnalysis.ErrorData { get; }

        public override string ToString()
        {
            string text = this?.PrimaryVideoStream?.CodecName?.ToUpper();
            string text2 = this?.PrimaryAudioStream?.CodecName?.ToUpper();
            string text3 = PrimarySubtitleStream?.CodecName?.ToUpper();
            int valueOrDefault = (this?.PrimaryVideoStream?.Width).GetValueOrDefault();
            int valueOrDefault2 = (this?.PrimaryVideoStream?.Height).GetValueOrDefault();
            return (string.IsNullOrWhiteSpace(text) ? "" : ("视频:" + text + "   ")) + (string.IsNullOrWhiteSpace(text2) ? "" : ("音频:" + text2 + "   ")) + (string.IsNullOrWhiteSpace(text3) ? "" : ("字幕:" + text3 + "   ")) + ((valueOrDefault <= 0) ? "" : $"分辨率:{valueOrDefault}x{valueOrDefault2}   ");
        }
    }
}
