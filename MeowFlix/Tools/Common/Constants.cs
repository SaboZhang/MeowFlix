namespace MeowFlix.Common;
public static class Constants
{
    public static readonly string AppName = AssemblyInfoHelper.GetAppInfo().NameAndVersion;
    public static readonly string RootDirectoryPath = Path.Combine(PathHelper.GetLocalFolderPath(), AppName);
    public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
    public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Log.txt");
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");
    public static readonly string DEFAULT_CHANNEL= "Assets/Files/MeowFlix-Channel.json";
    public static readonly string DEFAULT_MEDIA_SERVER_PATH = "Assets/Files/MeowFlix-MediaServers.json";
    public static readonly string DEFAULT_USERNAME = "localhost";
    public static readonly string ENCRYPT_CHANNEL = "私密";

    public static readonly string[] FileNameRegex = [@"(?<Name>[\w\s\.'’-]+?)\.(?<Year>\d{4})(?![pP])", @"^\d+\.(?<Name>.+)|^(?<Name>\D+)$", @"^(?:\[?(?<Name>[^\[\]\d]+?)\]?)?(?:\.(?<Year>\d{4})\b)?(?:\s*(?:BD|UHD|HD)?\d{3,4}[pP])?(?:\s*x265)?(?:\s*10bit)?(?:\s*AAC(?:\s*5\.1)?)?(?:\s*-\w+@[\w\s]+)?\.\w+$"];
    public static readonly string[] SeriesNameRegex = [@"^(?<Name>.*?)(?:\.\d{4}\.|_)?S(?<SeasonNumber>\d{1,3})E(?<EpisodeNumber>\d{1,3})(?:.*?(?<Year>\d{4}))?", @"\[.*?\]|\(.*?\)|\{.*?\}|(.*?\.)第[一二三四五六七八九十]季\.(.*)"];
    public const string IMDBTitleAPI = "http://www.omdbapi.com/?t={0}&apikey=2a59a17e";
    public const string IMDBBaseUrl = "https://www.imdb.com/title/{0}";
    public const string CineMaterialBaseUrl = "https://www.cinematerial.com";
    public const string CineMaterialBoxOffice = "https://www.cinematerial.com/titles/box-office";
    public const string CineMaterialPosters = "https://www.cinematerial.com/search?q={0}";

    public static readonly string[] FileExtensions = new string[] { ".m4v", ".3g2", ".3gp", ".nsv", ".tp", ".ts", ".ty", ".strm", ".pls", ".rm", ".rmvb", ".mpd", ".m3u", ".m3u8", ".mov",
            ".qt", ".divx", ".xvid", ".bivx", ".vob", ".nrg", ".img", ".iso", ".udf", ".pva", ".wmv", ".asf", ".asx", ".ogm", ".m2v", ".avi",
            ".mpg", ".mpeg", ".mp4", ".mkv", ".mk3d", ".avc", ".vp3", ".svq3", ".nuv", ".viv", ".dv", ".fli", ".flv", ".001",
            ".wpl", ".xspf", ".vdr", ".dvr-ms", ".xsp", ".mts", ".m2t", ".m2ts", ".evo", ".ogv", ".sdp", ".avs", ".rec",
            ".pxml", ".vc1", ".h264", ".rcv", ".rss", ".mpls", ".mpl", ".webm", ".bdm", ".wtv", ".trp", ".f4v" };

    public static readonly string[] ImageFileExtensions = new string[] { ".jpg", ".jpeg", ".png" };

    public const string SubsceneSearchAPI = "{0}/subtitles/searchbytitle?query={1}&l=";
    public const string AppCenterKey = "9f0c2c2b-0910-4cf7-bdae-4c1841bdfb0f";
}
