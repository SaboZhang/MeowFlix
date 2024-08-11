using FFmpeg.AutoGen;
using MeowFlix.Database;
using MeowFlix.Database.Tables;
using MeowFlix.Naming;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace MeowFlix.Common;
public sealed class VideoFileHelper
{
    /// <summary>
    /// 通过路径获取电影信息
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// 
    public static async Task<IEnumerable<VideoInfo>> ParseFilePathAsync(IEnumerable<string> fileNames, ServerType serverType, NamingOptions namingOptions)
    {
        if (fileNames == null || !fileNames.Any())
        {
            throw new ArgumentNullException(nameof(fileNames));
        }

        var tasks = fileNames.AsParallel().Select(async file =>
        {
            if (await ExistsAsync(file, serverType))
            {
                return null;
            }

            var videoInfos = new List<VideoInfo>();
            string fileName = Path.GetFileName(file);

            var mediaName = Path.GetFileNameWithoutExtension(fileName);

            int? year = null;

            if (ServerType.Movies == serverType)
            {
                var cleanDateTimeResult = CleanDateTime(mediaName, namingOptions);
                mediaName = cleanDateTimeResult.Name;
                year = cleanDateTimeResult.Year;

                if (TryCleanString(mediaName, namingOptions, out var newName))
                {
                    mediaName = newName;
                }
            }

            int? season = null;
            int? episode = null;

            if (ServerType.Series == serverType)
            {
                var episodePathParser = new EpisodePathParser(namingOptions);
                var episodePath = episodePathParser.Parse(file, false);
                mediaName = episodePath.SeriesName;
                season = episodePath.SeasonNumber;
                episode = episodePath.EpisodeNumber;
                year = episodePath.Year;
            }
            if (mediaName != null)
            {
                mediaName = mediaName.TrimStart('.', '[').Replace(".", " ");
            }
            var fileInfo = new FileInfo(file);
            var fileSize = fileInfo.Length;
            var ffmpegInfo = await GetVideoCodecAsync(file);

            var videoInfo = new VideoInfo(file, mediaName, fileName, year, fileSize, season, episode, ffmpegInfo);
            videoInfos.Add(videoInfo);

            return videoInfos;
        });

        var allVideoInfos = (await Task.WhenAll(tasks)).Where(v => v != null);
        return allVideoInfos.SelectMany(videoInfos => videoInfos);
    }

    /// <summary>
    /// 获取视频编码
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static Task<FFmpegInfo> GetVideoCodecAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return Task.FromResult<FFmpegInfo>(null);
        }

        return Task.Run(() =>
        {
            var info = new FFmpegInfo();

            FFmpegBinariesHelper.RegisterFFmpegBinaries();
            DynamicallyLoadedBindings.Initialize();
            SetupLogging();
            try
            {
                var videoStrem = new VideoStreamDecoder(path, AVMediaType.AVMEDIA_TYPE_VIDEO);
                var audioStream = new VideoStreamDecoder(path, AVMediaType.AVMEDIA_TYPE_AUDIO);
                info.Container = videoStrem.Container;
                info.VideoCodec = videoStrem.CodecName.ToUpper();
                info.AudioCodec = audioStream.CodecName.ToUpper();
                info.Duration = videoStrem.Duration;
                info.AudioLanguage = audioStream.GetAudioLanguages();
                info.SubtitleLanguage = videoStrem.GetVideoSubtitles();
                info.Quality = GetResolutionLabel(videoStrem.FrameSize);
                info.ChannelsCount = videoStrem.GetVideoChannelCount();
            }
            catch (Exception ex)
            {
                Logger.Error($"解析文件{path}时出错");
                Logger.Error(ex.Message);
            }

            return info;
        });
    }

    /// <summary>
    /// 视频清晰度
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    private static string GetResolutionLabel(Size size)
    {
        if (size.Width >= 3840 && size.Height >= 2160)
        {
            return "4K";
        }
        else if (size.Width >= 2560 && size.Height >= 1440)
        {
            return "2K";
        }
        else if (size.Width >= 1920 && size.Height >= 1080)
        {
            return "FHD";
        }
        else if (size.Width >= 1280 && size.Height >= 720)
        {
            return "HD";
        }
        else
        {
            return "SD";
        }
    }

    /// <summary>
    /// ffmpeg日志
    /// </summary>
    private static unsafe void SetupLogging()
    {
        ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);

        // do not convert to local function
        av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
        {
            if (level > ffmpeg.av_log_get_level()) return;

            var lineSize = 1024;
            var lineBuffer = stackalloc byte[lineSize];
            var printPrefix = 1;
            ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
            var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(line);
            Console.ResetColor();
            Logger.Information(line);
        };

        ffmpeg.av_log_set_callback(logCallback);
    }

    public static bool TryCleanString([NotNullWhen(true)] string? name, NamingOptions namingOptions, out string newName)
    {
        return CleanStringParser.TryClean(name, namingOptions.CleanStringRegexes, out newName);
    }

    public static CleanDateTimeResult CleanDateTime(string name, NamingOptions namingOptions)
    {
        return CleanDateTimeParser.Clean(name, namingOptions.CleanDateTimeRegexes);
    }

    /// <summary>
    /// 查询是否存在nfo文件,如果存在读取并返回true
    /// </summary>
    /// <param name="path"></param>
    /// <param name="serverType"></param>
    /// <returns></returns>
    private static async Task<bool> ExistsAsync(string path, ServerType serverType)
    {
        if (string.IsNullOrEmpty(path)) return false;
        bool exists = false;

        if (ServerType.Movies == serverType)
        {
            string movieNfoFile = Directory.GetFiles(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".nfo")
                                       .FirstOrDefault(nfoFile => IsXmlFile(nfoFile) && !IsFileAlreadyParsedAsync(nfoFile).Result);

            if (!string.IsNullOrEmpty(movieNfoFile))
            {
                await ReadNfoXmlFileAsync(movieNfoFile, serverType, path);
                exists = true;
            }
        }
        else if (ServerType.Series == serverType)
        {
            string parentDirectory = Directory.GetParent(path)?.FullName;
            if (!string.IsNullOrEmpty(parentDirectory) && Directory.Exists(parentDirectory))
            {
                string targetFile = FindFileInParentDirectories(parentDirectory, "tvshow.nfo");
                if (File.Exists(targetFile) && IsXmlFile(targetFile) && !IsFileAlreadyParsedAsync(targetFile).Result)
                {
                    await ReadNfoXmlFileAsync(targetFile, serverType, path);
                    exists = true;
                }
            }
        }

        return exists;
    }

    private static bool IsXmlFile(string filePath)
    {
        // 判断文件扩展名是否为 ".nfo"
        return Path.GetExtension(filePath)?.Equals(".nfo", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private static HashSet<string> parsedXmlFiles = new HashSet<string>();

    private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1); // 控制对 parsedXmlFiles 的访问

    private static async Task<bool> IsFileAlreadyParsedAsync(string filePath)
    {
        // 判断文件是否已经解析过
        await semaphoreSlim.WaitAsync();
        try
        {
            return parsedXmlFiles.Contains(filePath);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    private static async Task MarkFileAsParsedAsync(string filePath)
    {
        // 将文件标记为已解析
        await semaphoreSlim.WaitAsync();
        try
        {
            parsedXmlFiles.Add(filePath);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    private static string FindFileInParentDirectories(string startingDirectory, string fileName)
    {
        string currentDirectory = startingDirectory;

        while (!string.IsNullOrEmpty(currentDirectory))
        {
            string filePath = Path.Combine(currentDirectory, fileName);

            if (File.Exists(filePath))
            {
                return filePath; // 找到匹配的文件，返回全路径
            }

            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        }

        return null; // 没有找到匹配的文件
    }



    private static async Task ReadNfoXmlFileAsync(string nfoPath, ServerType serverType, string filePath)
    {
        switch (serverType)
        {
            case ServerType.Movies:
                await ReadMovieNfo(nfoPath, filePath);
                break;
            case ServerType.Series:
                ReadSeriesNfo(nfoPath);
                break;
        }
    }

    private static void WriteNfoXmlFile(string nfoPath, ServerType serverType)
    {

    }

    private static async Task ReadMovieNfo(string nfoPath, string filePath)
    {
        if (string.IsNullOrEmpty(nfoPath)) return;
        try
        {
            using var db = new AppDbContext();
            XmlDocument xmlDoc = new();
            xmlDoc.Load(nfoPath);
            XmlNode movieNode = xmlDoc.SelectSingleNode("/movie");
            string title = GetNodeText(movieNode, "title");
            string plot = GetNodeText(movieNode, "plot");
            string outline = GetNodeText(movieNode, "outline");
            string rating = GetNodeText(movieNode, "rating");
            int year = GetNodeInt(movieNode, "year");
            FileInfo fi = new(filePath);
            long size = fi.Length;
            string character = "";
            if (double.Parse(rating) > 8)
            {
                character = "高分";
            }

            var movie = new MovieTable(title, filePath, DateTime.Now.ToString(), ConvertSize(size), ServerType.Movies, character, year)
            {
                Rating = rating,
                Description = plot,
                Country = GetNodeText(movieNode, "country"),
                Mpaa = GetNodeText(movieNode, "mpaa"),
                SortTitle = GetNodeText(movieNode, "sorttitle"),
                Imdbid = GetNodeText(movieNode, "imdbid"),
                Tmdbid = GetNodeText(movieNode, "tmdbid"),
                Runtime = GetNodeInt(movieNode, "runtime"),
                ReleaseDate = GetNodeText(movieNode, "releasedate"),
                Originaltitle = GetNodeText(movieNode, "originaltitle"),
                Genres = GetNodesText(movieNode, "genre"),
                Actors = GetActors(movieNode, "actor"),
                StreamDetails = GetStreametails(movieNode),
                Set = GetNodeText(movieNode, "set/name"),
                Poster = GetPosterPath(filePath),
                CreaterTime = DateTime.Now,
                Outline = outline,
                Banner = GetNodeTextByAttribute(movieNode, "thumb", "aspect", "banner"),
                Cover = GetNodeTextByAttribute(movieNode, "thumb", "aspect", "cover"),
                Logo = GetNodeTextByAttribute(movieNode, "thumb", "aspect", "logo"),
                Fanart = GetNodeText(movieNode, "fanart/thumb"),


            };
            await db.Movies.AddAsync(movie);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(nfoPath);
            Logger.Error(ex, ex.Message);
        }

    }

    public static string GetPosterPath(string path)
    {
        string directoryPath = Path.GetDirectoryName(path);
        if (directoryPath != null && Directory.Exists(directoryPath))
        {
            string videoFileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            string cacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "VideoImage");

            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }

            // 构建可能的图片文件名列表
            string[] possibleImageFileNames = {
                $"{videoFileNameWithoutExtension}", "cover", "poster",
                $"{videoFileNameWithoutExtension}-cover", $"{videoFileNameWithoutExtension}-poster"
            };

            var imageFiles = Directory.GetFiles(directoryPath)
                .Where(file => Constants.ImageFileExtensions.Contains(Path.GetExtension(file).ToLower()) &&
                               possibleImageFileNames.Any(name => Path.GetFileNameWithoutExtension(file).Equals(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (imageFiles.Count != 0)
            {
                // 选择第一个满足条件的文件返回
                string selectedImageFile = imageFiles.First();
                // 缓存图片到Data\image目录下，并将文件重命名为MD5值
                string cachedFileName = Path.Combine(cacheDirectory, GetMD5Hash(selectedImageFile) + Path.GetExtension(selectedImageFile));
                if (!File.Exists(cachedFileName))
                {
                    File.Copy(selectedImageFile, cachedFileName);
                }
                return selectedImageFile;
            }
            else
            {
                return "ms-appx:///Assets/Cover/Poster.png";
            }
        }
        else
        {
            Logger.Error("文件夹不存在：" + directoryPath);
            return null;
        }
    }

    public static string GetMD5Hash(string input)
    {
        // 计算MD5值
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // 将字节数组转换为字符串
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

    private static void ReadSeriesNfo(string nfoPath)
    {

    }

    private static string GetNodeText(XmlNode parentNode, string nodeName)
    {
        XmlNode node = parentNode.SelectSingleNode(nodeName);
        return node?.InnerText;
    }

    private static string GetNodeTextByAttribute(XmlNode parentNode, string nodeName, string attributeName, string attribute)
    {
        string xPath = $"{nodeName}[@{attributeName}='{attribute}']";
        XmlNode node = parentNode.SelectSingleNode(xPath);
        return node?.InnerText;
    }

    private static List<string> GetNodesText(XmlNode parentNode, string nodeName)
    {
        var list = new List<string>();
        XmlNodeList nodes = parentNode.SelectNodes(nodeName);
        if (nodes == null) return null;
        foreach (XmlNode node in nodes)
        {
            list.Add(node.InnerText);
        }
        return list;
    }

    private static FileInfoTable GetStreametails(XmlNode parentNode)
    {
        XmlNodeList subtitleNodes = parentNode.SelectNodes("fileinfo/streamdetails/subtitle");
        List<SubtitleTable> subtitles = [];
        if (subtitleNodes != null)
        {
            SubtitleTable subtitle = new();

            foreach (XmlNode subtitleNode in subtitleNodes)
            {
                subtitle.Codec = GetNodeText(subtitleNode, "codec");
                subtitle.Language = GetNodeText(subtitleNode, "language");
                subtitle.CreaterTime = DateTime.Now;
                subtitles.Add(subtitle);
            }
        }
        XmlNode videoNode = parentNode.SelectSingleNode("fileinfo/streamdetails/video");
        XmlNode audioNode = parentNode.SelectSingleNode("fileinfo/streamdetails/audio");
        var streamDetails = new FileInfoTable
        {
            VideoWidth = GetNodeText(videoNode, "width"),
            VideoHeight = GetNodeText(videoNode, "height"),
            VideoCodec = GetNodeText(videoNode, "codec"),
            VideoBitrate = GetNodeText(videoNode, "bitrate"),
            Language = GetNodeText(videoNode, "language"),
            Aspect = GetNodeText(videoNode, "aspect"),
            VideoFrameRate = GetNodeText(videoNode, "audiobitrate"),
            VideoDuration = GetNodeText(videoNode, "duration"),
            MiCodec = GetNodeText(videoNode, "micodec"),
            AudioCodec = GetNodeText(audioNode, "codec"),
            AudioMiCodec = GetNodeText(audioNode, "micodec"),
            AudioBitrate = GetNodeText(audioNode, "bitrate"),
            AudioChannels = GetNodeText(audioNode, "channels"),
            AspectRatio = GetNodeText(videoNode, "aspectratio"),
            SampLingrate = GetNodeText(videoNode, "samplerate"),
            Subtitles = subtitles,
            VideoResolution = GetResolutionLabel(new Size(int.Parse(GetNodeText(videoNode, "width")), int.Parse(GetNodeText(videoNode, "height")))),
        };


        return streamDetails;
    }

    private static List<MediaActorTable> GetActors(XmlNode parentNode, string actorNodeName)
    {
        var actors = new List<MediaActorTable>();

        // 处理 <actor> 节点
        XmlNodeList actorNodes = parentNode.SelectNodes(actorNodeName);
        if (actorNodes != null)
        {
            foreach (XmlNode actorNode in actorNodes)
            {
                var actor = new MediaActorTable
                {
                    Actor = GetNodeText(actorNode, "name"),
                    Role = GetNodeText(actorNode, "role")
                };

                // 查找关联的 <thumb> 节点
                XmlNode associatedThumbNode = actorNode.SelectSingleNode("preceding-sibling::producer/thumb");
                if (associatedThumbNode != null)
                {
                    actor.Thumb = GetNodeText(associatedThumbNode, "thumb");
                }

                actors.Add(actor);
            }
        }
        XmlNodeList producerNodes = parentNode.SelectNodes("producer");
        if (producerNodes != null)
        {
            foreach (XmlNode producerNode in producerNodes)
            {
                var actor = new MediaActorTable
                {
                    Actor = GetNodeText(producerNode, "name"),
                    Role = "",  // producer 节点没有 role
                    Thumb = GetNodeText(producerNode, "thumb"),
                    Profile = GetNodeText(producerNode, "profile")
                };

                actors.Add(actor);
            }
        }

        return actors;
    }


    private static int GetNodeInt(XmlNode parentNode, string nodeName)
    {
        string valueStr = GetNodeText(parentNode, nodeName);
        return int.TryParse(valueStr, out int value) ? value : 0;
    }

    private static double ConvertSize(long size)
    {
        double fileSizeInKB = (double)size / 1024;  // 1 KB = 1024 bytes
        double fileSizeInMB = fileSizeInKB / 1024;  // 1 MB = 1024 KB
        double fileSizeInGB = fileSizeInMB / 1024;  // 1 GB = 1024 MB
        return Math.Round(fileSizeInGB, 2);
    }
}
