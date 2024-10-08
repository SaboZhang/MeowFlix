﻿using MeowFlix.Database;
using MeowFlix.Database.Tables;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MeowFlix.ViewModels;
public partial class BackupSettingViewModel : ObservableObject
{
    private bool isMediaServer;

    [ObservableProperty]
    public string infoBarTitle;

    [ObservableProperty]
    public InfoBarSeverity infoBarSeverity = InfoBarSeverity.Informational;

    [RelayCommand]
    private async Task OnBackupServer(object isMediaServer)
    {
        try
        {
            this.isMediaServer = Convert.ToBoolean(isMediaServer);
            var fileName = $"MeowFlix-MediaServers-{DateTime.Now:yyyy-MM-dd HH-mm-ss}";

            if (!this.isMediaServer)
            {
                fileName = $"MeowFlix-Channels-{DateTime.Now:yyyy-MM-dd HH-mm-ss}";
            }

            var fileTypeChoices = new Dictionary<string, IList<string>>
            {
                { "Json", new List<string>() { ".json" } }
            };
            var file = await FileAndFolderPickerHelper.PickSaveFileAsync(App.currentWindow, fileTypeChoices, fileName);

            if (file != null)
            {
                using var db = new AppDbContext();
                dynamic servers = await db.MediaServers.ToListAsync();

                if (!this.isMediaServer)
                {
                    servers = await db.Chanels.ToListAsync();
                }

                var json = JsonConvert.SerializeObject(servers, Formatting.Indented);
                using (var outfile = new StreamWriter(file.Path))
                {
                    await outfile.WriteAsync(json);
                }

                InfoBarTitle = "Backup completed successfully";
                InfoBarSeverity = InfoBarSeverity.Success;
            }
        }
        catch (Exception ex)
        {
            InfoBarTitle = ex.Message;
            InfoBarSeverity = InfoBarSeverity.Error;
            Logger?.Error(ex, "BackupSettingViewModel: Backup Servers");
        }
    }

    [RelayCommand]
    private async Task OnRestoreServer(object isMediaServer)
    {
        try
        {
            this.isMediaServer = Convert.ToBoolean(isMediaServer);

            var file = await FileAndFolderPickerHelper.PickSingleFileAsync(App.currentWindow, new string[] { ".json" });
            if (file != null)
            {
                using var streamReader = File.OpenText(file.Path);
                var json = await streamReader.ReadToEndAsync();
                var content = JsonConvert.DeserializeObject<List<BaseServerTable>>(json);
                if (content is not null)
                {
                    using var db = new AppDbContext();
                    if (this.isMediaServer)
                    {
                        foreach (var item in content)
                        {
                            await db.MediaServers.AddAsync(new MediaServerTable(item.Title, item.FilePath, item.IsActive, item.ServerType));
                        }
                    }
                    else
                    {
                        foreach (var item in content)
                        {
                            await db.Chanels.AddAsync(new ChannelTable(item.Title, item.FilePath, item.IsActive));
                        }
                    }

                    await db.SaveChangesAsync();
                    InfoBarTitle = "Restore completed successfully";
                    InfoBarSeverity = InfoBarSeverity.Success;
                }
            }
        }
        catch (Exception ex)
        {
            InfoBarTitle = ex.Message;
            InfoBarSeverity = InfoBarSeverity.Error;
            Logger?.Error(ex, "BackupSettingViewModel: Restore Servers");
        }
    }

    [RelayCommand]
    private async Task OnBackupSettings()
    {
        try
        {
            var fileTypeChoices = new Dictionary<string, IList<string>>
            {
                { "Json", new List<string>() { ".json" } }
            };
            var suggestedFileName = $"MeowFlix-Settings-{DateTime.Now:yyyy-MM-dd HH-mm-ss}";

            var file = await FileAndFolderPickerHelper.PickSaveFileAsync(App.currentWindow, fileTypeChoices, suggestedFileName);

            if (file != null)
            {
                var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                using (var outfile = new StreamWriter(file.Path))
                {
                    await outfile.WriteAsync(json);
                }
                InfoBarTitle = "Backup completed successfully";
                InfoBarSeverity = InfoBarSeverity.Success;
            }
        }
        catch (Exception ex)
        {
            InfoBarTitle = ex.Message;
            InfoBarSeverity = InfoBarSeverity.Error;
            Logger?.Error(ex, "BackupSettingViewModel: Backup Settings");
        }
    }

    [RelayCommand]
    private async Task OnRestoreSettings()
    {
        try
        {
            var file = await FileAndFolderPickerHelper.PickSingleFileAsync(App.currentWindow, new string[] { ".json" });

            if (file != null)
            {
                using var streamReader = File.OpenText(file.Path);
                var json = await streamReader.ReadToEndAsync();
                var content = JsonConvert.DeserializeObject<AppConfig>(json);
                if (content is not null)
                {
                    Settings = content;
                    Settings.Save();
                    InfoBarTitle = "Restore completed successfully";
                    InfoBarSeverity = InfoBarSeverity.Success;
                }
            }
        }
        catch (Exception ex)
        {
            InfoBarTitle = ex.Message;
            InfoBarSeverity = InfoBarSeverity.Error;
            Logger?.Error(ex, "BackupSettingViewModel: Restore Settings");
        }
    }
}
