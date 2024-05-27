using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MeowFlix.Common;

public sealed unsafe class VideoStreamDecoder : IDisposable
{
    private readonly AVCodecContext* _pCodecContext;
    private readonly AVFormatContext* _pFormatContext;
    private readonly AVFrame* _receivedFrame;
    private readonly int _streamIndex;
    private readonly AVStream* _pAudioStream;
    public VideoStreamDecoder(string path, AVMediaType aVMediaType)
    {
        _pFormatContext = ffmpeg.avformat_alloc_context();
        _receivedFrame = ffmpeg.av_frame_alloc();
        var pFormatContext = _pFormatContext;
        ffmpeg.avformat_open_input(&pFormatContext, path, null, null).ThrowExceptionIfError();
        ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();
        AVCodec* codec = null;
        _streamIndex = ffmpeg
            .av_find_best_stream(_pFormatContext, aVMediaType, -1, -1, &codec, 0)
            .ThrowExceptionIfError();
        _pCodecContext = ffmpeg.avcodec_alloc_context3(codec);

        ffmpeg.avcodec_parameters_to_context(_pCodecContext, _pFormatContext->streams[_streamIndex]->codecpar)
            .ThrowExceptionIfError();
        ffmpeg.avcodec_open2(_pCodecContext, codec, null).ThrowExceptionIfError();

        CodecName = ffmpeg.avcodec_get_name(codec->id);
        FrameSize = new Size(_pCodecContext->width, _pCodecContext->height);
        int audioStreamIndex = -1;
        for (int i = 0; i < _pFormatContext->nb_streams; i++)
        {
            if (_pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                _pAudioStream = _pFormatContext->streams[i];
                audioStreamIndex = i;
                break;
            }
        }
    }

    public string CodecName { get; }
    public Size FrameSize { get; }
    public long Duration => _pFormatContext->duration / ffmpeg.AV_TIME_BASE;
    public string Container => Marshal.PtrToStringAnsi((IntPtr)_pFormatContext->iformat->name);

    public void Dispose()
    {
        ffmpeg.avcodec_close(_pCodecContext);
        var pFormatContext = _pFormatContext;
        ffmpeg.avformat_close_input(&pFormatContext);
    }

    public unsafe int GetVideoChannelCount()
    {
        int channelCount = 0;

        for (int i = 0; i < _pFormatContext->nb_streams; i++)
        {
            if (_pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                AVCodecParameters* audioCodecParams = _pFormatContext->streams[i]->codecpar;
                channelCount = audioCodecParams->ch_layout.nb_channels;
                break;
            }
        }
        return channelCount;
    }

    public unsafe List<string> GetAudioLanguages()
    {
        var languages = new List<string>();

        AVDictionaryEntry* entry = null;
        while ((entry = ffmpeg.av_dict_get(_pAudioStream->metadata, "", entry, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
        {
            if (entry->key != null && entry->value != null)
            {
                // 判断包含语言信息的键
                if (Marshal.PtrToStringAnsi((IntPtr)entry->key).Contains("lang", StringComparison.OrdinalIgnoreCase))
                {
                    languages.Add(Marshal.PtrToStringAnsi((IntPtr)entry->value));
                }
            }
        }

        return languages;
    }

    public unsafe List<string> GetVideoSubtitles()
    {
        List<string> subtitles = [];

        for (int i = 0; i < _pFormatContext->nb_streams; i++)
        {
            AVStream* stream = _pFormatContext->streams[i];
            if (stream->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_SUBTITLE)
            {
                AVDictionaryEntry* entry = null;
                while ((entry = ffmpeg.av_dict_get(stream->metadata, "", entry, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
                {
                    if (Marshal.PtrToStringAnsi((IntPtr)entry->key) == "language")
                    {
                        string subtitleLanguage = Marshal.PtrToStringAnsi((IntPtr)entry->value);
                        subtitles.Add(subtitleLanguage);
                    }
                }
            }
        }

        return subtitles;
    }
}
