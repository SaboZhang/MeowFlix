using CommunityToolkit.Labs.WinUI;

using Nucs.JsonSettings;
using Nucs.JsonSettings.Autosave;
using Nucs.JsonSettings.Fluent;
using Nucs.JsonSettings.Modulation;
using Nucs.JsonSettings.Modulation.Recovery;

using System.Diagnostics;
using System.Text.RegularExpressions;

using MeowFlix.Database.Tables;
using MeowFlix.Tools;

namespace MeowFlix.Common;
public static partial class AppHelper
{
    public static AppConfig Settings = JsonSettings.Configure<AppConfig>()
                               .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                               .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                               .LoadNow()
                               .EnableAutosave();

    public static ObservableCollection<TokenItem> SubtitleLanguageCollection()
    {
        return new ObservableCollection<TokenItem>
        {
            new TokenItem{Content ="Persian", IsSelected = true},
            new TokenItem{Content ="English", IsSelected = true},
            new TokenItem{Content ="Albanian"},
            new TokenItem{Content ="Arabic"},
            new TokenItem{Content ="Bengali"},
            new TokenItem{Content ="Brazillian"},
            new TokenItem{ Content ="Burmese"},
            new TokenItem{ Content ="Croatian"},
            new TokenItem{ Content ="Danish"},
            new TokenItem{ Content ="Dutch"},
            new TokenItem{ Content ="Finnish"},
            new TokenItem{ Content ="French"},
            new TokenItem{ Content ="German"},
            new TokenItem{ Content ="Hebrew"},
            new TokenItem{ Content ="Hindi"},
            new TokenItem{ Content ="Indonesian"},
            new TokenItem{ Content ="Italian"},
            new TokenItem{ Content ="Japanese"},
            new TokenItem{ Content ="Korean"},
            new TokenItem{ Content ="Malay"},
            new TokenItem{ Content ="Malayalam"},
            new TokenItem{ Content ="Morwegian"},
            new TokenItem{ Content ="Romanian"},
            new TokenItem{ Content ="Russian"},
            new TokenItem{ Content ="Serbian"},
            new TokenItem{ Content ="Spanish"},
            new TokenItem{ Content ="Swedish"},
            new TokenItem{ Content ="Tamil"},
            new TokenItem{ Content ="Thai"},
            new TokenItem{ Content ="Turkish"},
            new TokenItem{ Content ="Urdu"},
            new TokenItem{ Content ="Vietnamese"},
            new TokenItem{ Content ="Hungarian"},
            new TokenItem{ Content ="Portuguese"}
        };
    }

    public static ObservableCollection<string> GenerateTextBlockStyles()
    {
        return new ObservableCollection<string>
        {
            "BaseTextBlockStyle",
            "BodyStrongTextBlockStyle",
            "BodyTextBlockStyle",
            "CaptionTextBlockStyle",
            "DisplayTextBlockStyle",
            "HeaderTextBlockStyle",
            "SubheaderTextBlockStyle",
            "SubtitleTextBlockStyle",
            "TitleLargeTextBlockStyle",
            "TitleTextBlockStyle"
        };
    }

    public static string RemoveSpecialWords(string stringToClean)
    {
        if (!string.IsNullOrEmpty(stringToClean))
        {
            var wordFilter = new Regex(Constants.FileNameRegex[0], RegexOptions.IgnoreCase);
            var cleaned = wordFilter.Replace(stringToClean, " ").Trim();
            cleaned = CleanRegex().Replace(cleaned, " "); // remove space [More than 2 space] and replace with one space

            return cleaned.Trim();
        }
        else { return stringToClean; }
    }

    public static void LaunchIDM(string idmPath, string link)
    {
        if (string.IsNullOrEmpty(idmPath) || string.IsNullOrEmpty(link))
        {
            return;
        }

        Process.Start(idmPath, $"/d \"{link}\"");
    }

    public static bool IsUrlFile(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var extension = Path.GetExtension(uri.AbsolutePath);
            if (!string.IsNullOrEmpty(extension))
            {
                return true;
            }
        }

        return false;
    }

    /*public static void CreateIMDBDetailsWindow(string query)
    {
        var window = new IMDBDetailsWindow(query);
        IThemeService ThemeService = new ThemeService();
        ThemeService.Initialize(window);
        ThemeService.ConfigBackdrop();
        ThemeService.ConfigElementTheme();
        window.Activate();
    }*/

    public static string GetServerUrlWithoutLeftAndRightPart(string url)
    {
        if (string.IsNullOrEmpty(url) || !IsValidUri(url))
        {
            return null;
        }
        else
        {
            var uri = new Uri(url);
            var host = uri.Host;
            var parts = host.Split('.');
            return string.Join(".", parts.Take(parts.Length - 1));
        }
    }

    public static bool IsValidUri(string uriString)
    {
        Uri uriResult;
        return Uri.TryCreate(uriString, UriKind.Absolute, out uriResult);
    }

    public static string GetServerUrlWithoutRightPart(string url)
    {
        if (string.IsNullOrEmpty(url) || !IsValidUri(url))
        {
            return null;
        }
        else
        {
            var uri = new Uri(url);
            return uri.Scheme + "://" + uri.Host;
        }
    }

    [GeneratedRegex("[ ]{2,}")]
    private static partial Regex CleanRegex();

    public static void TokenViewSelectionChanged(BaseViewModel viewModel, TokenView token, SelectionChangedEventArgs e, Action OnTokenFilter)
    {
        if (viewModel.DataListACV == null)
        {
            return;
        }

        if (token != null)
        {
            dynamic selectedItem = e.AddedItems.Count > 0 ? e.AddedItems[0] as TokenItem : null;

            selectedItem ??= e.RemovedItems.Count > 0 ? e.RemovedItems[0] as TokenItem : null;

            if (selectedItem == null)
            {
                return;
            }

            if (token.SelectedItems.Count == 0)
            {
                var allItem = token.Items[0] as TokenItem;
                allItem.IsSelected = true;
                selectedItem = allItem;
            }

            if (selectedItem.Content.ToString().Equals("All") && selectedItem.IsSelected)
            {
                foreach (TokenItem item in token.Items)
                {
                    if (item.Content.ToString().Equals("All"))
                    {
                        continue;
                    }
                    item.IsSelected = false;
                }

                viewModel.DataListACV.Filter = null;
            }
            else if (!selectedItem.Content.ToString().Equals("All"))
            {
                foreach (TokenItem item in token.Items)
                {
                    if (item.Content.ToString().Equals("All") && item.IsSelected)
                    {
                        item.IsSelected = false;
                    }
                    break;
                }

                OnTokenFilter?.Invoke();
            }

            viewModel.DataListACV.RefreshFilter();
        }
    }

    public static bool ContinueIfWrongData(string title, string filePath, string link, BaseMediaTable baseMedia)
    {
        if (string.IsNullOrEmpty(title) || filePath.Equals($"{baseMedia?.FilePath}../") ||
            title.Equals("[To Parent Directory]") || title.Equals("../") || filePath.Contains("?C=") ||
            ((filePath.Contains("fbserver")) && link.Contains("?C=")))
        {
            return true;
        }
        return false;
    }

    public static string FixTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            return title;

        title = Regex.Replace(title, @"\s*<.*?>\s*", "", RegexOptions.Singleline);
        title = title.Replace(".E..&gt;", "").Replace(">", "");
        title = RemoveSpecialWords(GeneralHelper.GetDecodedStringFromHtml(title));
        return title;
    }

    private static string ConcatenateUrls(string baseUrl, string relativeUrl)
    {
        // Ensure baseUrl ends with a slash
        if (!baseUrl.EndsWith("/"))
        {
            baseUrl += "/";
        }

        if (relativeUrl.StartsWith("/"))
        {
            relativeUrl = relativeUrl.Substring(1);
        }

        // Create a base URI from the baseUrl string
        Uri baseUri = new Uri(baseUrl);

        // Create a new URI by combining the base URI with the relativeUrl
        Uri combinedUri = new Uri(baseUri, relativeUrl);

        // Get the combined URL as a string
        string resultUrl = combinedUri.ToString();

        return resultUrl;
    }

    public static string FixUriDuplication(string baseUrl, string relativeUrl)
    {
        var baseUrlUri = new Uri(baseUrl);

        var baseUrlLeftPart = baseUrlUri.GetLeftPart(UriPartial.Authority);
        var duplicateSection = baseUrl.Replace(baseUrlLeftPart, "");

        if (duplicateSection.StartsWith("/"))
        {
            duplicateSection = duplicateSection.Substring(1);
        }
        if (relativeUrl.Contains(duplicateSection))
        {
            baseUrl = baseUrl.Replace(duplicateSection, "");
        }
        return ConcatenateUrls(baseUrl, relativeUrl);
    }

    public static string GetPlayerPath()
    {

        return string.Empty;
    }

    public static void LaunchPlayer(string playerPath, string filePath)
    {
        if (string.IsNullOrEmpty(playerPath) || string.IsNullOrEmpty(filePath))
        {
            return;
        }

        Process.Start(playerPath, filePath);
    }

    public static async void LaunchPlayer(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }
        
        filePath = "http://192.168.31.10:5244/d/cadd%E7%94%B5%E5%BD%B1/The.Marvels.2023.2160p.BluRay.x265.10bit.Atmos.TrueHD7.1-WiKi/The.Marvels.2023.2160p.BluRay.x265.10bit.Atmos.TrueHD7.1-WiKi.mkv";
        
        if (!filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            Process.Start("explorer.exe", filePath);
        }
        
        var mediaPlayerDetector = new MediaPlayerDetector();
        var thirtyDays = new TimeSpan(30, 0, 0,0);
        var mediaPlayer = await mediaPlayerDetector.GetInstalledStandardMediaPlayerAsync(thirtyDays);
        switch (mediaPlayer)
        {
            case InstalledPlayer.PotPlayer:
                filePath = "potplayer://" + filePath;
                Process.Start("explorer.exe", filePath);
                break;
            case InstalledPlayer.VLC:
                filePath = "vlc://" + filePath;
                Process.Start("explorer.exe", filePath);
                break;
            case InstalledPlayer.None:
                Growl.Error("未找到支持的播放器，仅支持PotPlayer、VLC 直接播放WebDAV中的视频，其他播放器请挂载为本地磁盘播放");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        

    }

    public static void OpenFolderContainingFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }
        string folderPath = Path.GetDirectoryName(filePath);
        Process.Start("explorer.exe", folderPath);
    }
}

