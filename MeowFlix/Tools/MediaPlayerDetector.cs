using System.Collections.Concurrent;
using Microsoft.Win32;

namespace MeowFlix.Tools;

public enum InstalledPlayer
{
    None,
    VLC,
    PotPlayer
}

public class MediaPlayerDetector
{
    private readonly ConcurrentDictionary<string, InstalledPlayer> _cache = new ConcurrentDictionary<string, InstalledPlayer>();

    public async Task<InstalledPlayer> GetInstalledStandardMediaPlayerAsync(TimeSpan cacheDuration)
    {
        string cacheKey = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        if (_cache.TryGetValue(cacheKey, out InstalledPlayer cachedResult))
        {
            return cachedResult;
        }

        InstalledPlayer installedPlayer = await CheckMediaPlayerInstallationAsync();

        _cache.TryAdd(cacheKey, installedPlayer);
        _cache.TryRemove(cacheKey, out _); // Automatically remove after cacheDuration

        return installedPlayer;
    }

    private async Task<InstalledPlayer> CheckMediaPlayerInstallationAsync()
    {
        // 检查注册表中是否存在安装信息
        string vlcRegistryPath = @"SOFTWARE\VideoLAN\VLC";
        string potPlayerRegistryPath = @"SOFTWARE\DAUM\PotPlayer";
        using (RegistryKey vlcKey = Registry.CurrentUser.OpenSubKey(vlcRegistryPath))
        using (RegistryKey potPlayerKey = Registry.CurrentUser.OpenSubKey(potPlayerRegistryPath))
        {
            if (vlcKey != null)
            {
                return InstalledPlayer.VLC;
            }
            else if (potPlayerKey != null)
            {
                return InstalledPlayer.PotPlayer;
            }
        }

        // 检查系统盘下的标准安装路径
        string systemDrive = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2); // e.g., "C:"
        string dDrive = "D:";
        
        string[] systemStandardPaths =
        {
            Path.Combine(systemDrive, "Program Files", "VideoLAN", "VLC", "vlc.exe"),
            Path.Combine(systemDrive, "Program Files (x86)", "VideoLAN", "VLC", "vlc.exe"),
            Path.Combine(systemDrive, "Program Files", "DAUM", "PotPlayer", "PotPlayerMini64.exe"),
            Path.Combine(systemDrive, "Program Files (x86)", "DAUM", "PotPlayer", "PotPlayerMini64.exe")
        };
        
        string[] dStandardPaths =
        {
            Path.Combine(dDrive, "Program Files", "VideoLAN", "VLC", "vlc.exe"),
            Path.Combine(systemDrive, "Program Files (x86)", "VideoLAN", "VLC", "vlc.exe"),
            Path.Combine(dDrive, "Program Files", "DAUM", "PotPlayer", "PotPlayerMini64.exe"),
            Path.Combine(systemDrive, "Program Files (x86)", "DAUM", "PotPlayer", "PotPlayerMini64.exe")
        };
        
        var fileCheckTasks = new List<Task<bool>>();
        foreach (string path in systemStandardPaths.Concat(dStandardPaths))
        {
            fileCheckTasks.Add(Task.Run(() => File.Exists(path)));
        }

        await Task.WhenAll(fileCheckTasks);

        for (int i = 0; i < fileCheckTasks.Count; i++)
        {
            if (fileCheckTasks[i].Result)
            {
                return i == 0 ? InstalledPlayer.VLC : InstalledPlayer.PotPlayer;
            }
        }

        return InstalledPlayer.None;
    }
}