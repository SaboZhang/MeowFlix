using CommunityToolkit.WinUI.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using Newtonsoft.Json;
using MeowFlix.Database;
using MeowFlix.Database.Tables;
using MeowFlix.Views.ContentDialogs;
using MeowFlix.Naming;
using MeowFlix.Tools;

namespace MeowFlix.ViewModels;

public partial class ServerViewModel : ObservableRecipient, ITitleBarAutoSuggestBoxAware
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty] 
    public ObservableCollection<BaseServerTable> serverList;

    [ObservableProperty] 
    public AdvancedCollectionView serverListACV;

    [ObservableProperty] 
    private int segmentedSelectedIndex = 0;

    [ObservableProperty] 
    private string infoBarTitle;

    [ObservableProperty] 
    private bool infoBarIsOpen;

    [ObservableProperty] 
    private InfoBarSeverity infoBarSeverity;

    [ObservableProperty] 
    public string floderPath;

    [ObservableProperty] 
    public bool isPrivate = false;

    [ObservableProperty] 
    private bool pathTypeSelectedIndex = true;

    [ObservableProperty] 
    private bool webDavIsShow = false;

    [ObservableProperty] 
    private object cmbPathTypeSelectedItem;

    private string tempQuery;
    public IThemeService themeService;

    public ServerViewModel(IThemeService themeService)
    {
        this.themeService = themeService;
    }

    [RelayCommand]
    private async Task OnPageLoaded()
    {
        try
        {
            IsActive = true;
            await Task.Run(() =>
            {
                dispatcherQueue.TryEnqueue(async () =>
                {
                    using var db = new AppDbContext();
                    if (SegmentedSelectedIndex == 0)
                    {
                        ServerList = new(await db.Storages.ToListAsync());
                    }
                    else
                    {
                        ServerList = new(await db.SubtitleServers.ToListAsync());
                    }

                    ServerListACV = new AdvancedCollectionView(ServerList, true);
                    ServerListACV.SortDescriptions.Add(new SortDescription("Title", SortDirection.Ascending));
                });
            });
            IsActive = false;
        }
        catch (Exception ex)
        {
            IsActive = false;
            InfoBarSeverity = InfoBarSeverity.Error;
            InfoBarTitle = ex.Message;
            InfoBarIsOpen = true;
            Logger?.Error(ex, "ServerViewModel: OnPageLoad");
        }
    }

    [RelayCommand]
    private async Task OnSegmentedSelectionChanged()
    {
        await OnPageLoaded();
    }

    [RelayCommand]
    private async Task OnAddServer()
    {
        var dialog = new ServerContentDialog();
        dialog.ViewModel = this;
        dialog.ServerTitle = string.Empty;
        FloderPath = string.Empty;
        dialog.ServerActivation = true;
        dialog.CmbServerTypeSelectedItem = null;
        CmbPathTypeSelectedItem = null!;
        dialog.WebUserName = string.Empty;
        dialog.WebPassWord = string.Empty;
        dialog.WebUrl = string.Empty;
        dialog.Title = "添加媒体库";
        dialog.IsPrivate = IsPrivate;
        dialog.PrimaryButtonClick += OnAddServerPrimaryButton;
        dialog.Click += async (s, e) => { await OnSelectMediaPath(); };

        await dialog.ShowAsync();
    }

    [RelayCommand]
    private Task OnSelectPathType()
    {
        var cmb = (CmbPathTypeSelectedItem as ComboBoxItem)?.Tag.ToString();
        switch (cmb)
        {
            case "Local":
                PathTypeSelectedIndex = true;
                WebDavIsShow = false;
                break;
            case "WebDav":
                PathTypeSelectedIndex = false;
                WebDavIsShow = true;
                break;
            default:
                PathTypeSelectedIndex = true;
                WebDavIsShow = false;
                break;
        }

        return Task.CompletedTask;
    }


    private async Task OnSelectMediaPath()
    {
        try
        {
            var folder = await FileAndFolderPickerHelper.PickSingleFolderAsync(App.currentWindow);
            if (folder is not null)
            {
                using var db = new AppDbContext();
                // TODO: 检查是否已经存在
                var exists = db.Storages.Any(s => s.FilePath == Path.GetFullPath(folder.Path));
                if (exists)
                {
                    Growl.Warning("目录已存在！");
                    FloderPath = string.Empty;
                }
                else
                {
                    FloderPath = folder.Path;
                }
            }
        }
        catch (Exception ex)
        {
            InfoBarSeverity = InfoBarSeverity.Error;
            InfoBarTitle = ex.Message;
            InfoBarIsOpen = true;
            Logger?.Error(ex, "ServerViewModel: OnSelectMediaPath");
        }
    }
    
    private async void OnAddServerPrimaryButton(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        try
        {
            if (sender is null) return;
            if (!ValidateInput(sender, args))
            {
                args.Cancel = true;
                return;
            }

            if (WebDavIsShow)
            {
                var dialog = sender as ServerContentDialog;
                WebDavClient webDavClient = new(dialog.WebUrl, dialog.WebUserName, dialog.WebPassWord);
                var files = await webDavClient.RecursivelyListFilesAsync();
            }
            
            await using var db = new AppDbContext();
            await AddToDatabase(db, sender, args);

            UpdateUiAfterSuccess();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private bool ValidateInput(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var dialog = sender as ServerContentDialog;
        if (WebDavIsShow && IsDialogInputEmpty(dialog))
        {
            DisplayErrorMessage();
            return false;
        }

        if (IsServerInputEmpty(dialog))
        {
            DisplayErrorMessage();
            return false;
        }

        return true;
    }
    
    // 检查WebDav相关输入是否为空
    private bool IsDialogInputEmpty(ServerContentDialog dialog)
    {
        return string.IsNullOrEmpty(dialog.ServerTitle) &&
               string.IsNullOrEmpty(dialog.WebUrl) &&
               string.IsNullOrEmpty(dialog.WebUserName) &&
               string.IsNullOrEmpty(dialog.WebPassWord);
    }

    // 检查服务器相关输入是否为空
    private bool IsServerInputEmpty(ServerContentDialog dialog)
    {
        return string.IsNullOrEmpty(dialog.ServerTitle) &&
               string.IsNullOrEmpty(FloderPath);
    }

    // 显示错误消息
    private void DisplayErrorMessage()
    {
        Growl.Error("带*的内容为必填项！");
    }

    private async Task AddToDatabase(AppDbContext db, ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var dialog = sender as ServerContentDialog;

        var baseServer =
            new BaseServerTable(dialog.ServerTitle.Trim(), FloderPath, dialog.ServerActivation);

        if (SegmentedSelectedIndex == 0)
        {
            var type = GeneralHelper.GetEnum<ServerType>((dialog.CmbServerTypeSelectedItem as ComboBoxItem)?.Tag
                ?.ToString());
            baseServer.ServerType = type;

            await db.Storages.AddAsync(new StorageTable(baseServer.Title, baseServer.FilePath, baseServer.IsActive,
                baseServer.ServerType, IsPrivate));

            if (IsPrivate)
            {
                await AddDefaultAuthIfNotExists(db, dialog);
            }
        }
        else
        {
            await db.SubtitleServers.AddAsync(new SubtitleServerTable(baseServer.Title, baseServer.FilePath,
                baseServer.IsActive));
        }

        await db.SaveChangesAsync();
    }

    private async Task AddDefaultAuthIfNotExists(AppDbContext db, ServerContentDialog dialog)
    {
        var exists = db.Auth.Any(p => p.Username == Constants.DEFAULT_USERNAME);
        if (!exists)
        {
            var hashPassword = BCrypt.Net.BCrypt.HashPassword(dialog.Password.Trim());
            await db.Auth.AddAsync(new AuthTable(Constants.DEFAULT_USERNAME, hashPassword, true));
        }
    }

    private void UpdateUiAfterSuccess()
    {
        InfoBarSeverity = InfoBarSeverity.Success;
        InfoBarTitle = "新媒体库添加成功";
        InfoBarIsOpen = true;
        TryInvokeOnPageLoaded();
        Search(tempQuery);
    }

    private void HandleException(Exception ex)
    {
        InfoBarSeverity = InfoBarSeverity.Error;
        InfoBarTitle = ex.Message;
        InfoBarIsOpen = true;
        Logger?.Error(ex, "ServerViewModel: OnAddServerPrimaryButton");
    }

    private async void TryInvokeOnPageLoaded()
    {
        try
        {
            await OnPageLoaded();
        }
        catch (Exception ex)
        {
            // 可以选择记录日志或显示错误信息
            Logger?.Error(ex, "ServerViewModel: TryInvokeOnPageLoaded");
        }
    }


    [RelayCommand]
    private async Task OnUpdateServer(object dataContext)
    {
        try
        {
            if (dataContext != null)
            {
                var dialog = new ServerContentDialog();
                dialog.ViewModel = this;
                dialog.Title = "修改";

                var server = dataContext as BaseServerTable;
                dialog.ServerTitle = server.Title;
                FloderPath = server.FilePath;
                dialog.ServerActivation = server.IsActive;
                dialog.Click += async (s, e) => { await OnSelectMediaPath(); };

                if (SegmentedSelectedIndex == 0)
                {
                    dialog.CmbServerTypeSelectedItem = server.ServerType;
                }

                dialog.PrimaryButtonClick += async (s, e) =>
                {
                    if (string.IsNullOrEmpty(dialog.ServerTitle) && string.IsNullOrEmpty(FloderPath))
                    {
                        InfoBarSeverity = InfoBarSeverity.Error;
                        InfoBarTitle = "媒体库不能修改";
                        InfoBarIsOpen = true;
                        return;
                    }

                    using var db = new AppDbContext();

                    if (SegmentedSelectedIndex == 0)
                    {
                        var oldMediaServer = await db.MediaServers.Where(x =>
                            x.Title == server.Title && x.FilePath == server.FilePath &&
                            x.ServerType == server.ServerType).FirstOrDefaultAsync();
                        if (oldMediaServer != null)
                        {
                            var type = GeneralHelper.GetEnum<ServerType>(
                                (dialog.CmbServerTypeSelectedItem as ComboBoxItem).Content?.ToString());

                            oldMediaServer.Title = dialog.ServerTitle;
                            oldMediaServer.FilePath = FloderPath;
                            oldMediaServer.ServerType = type;
                            oldMediaServer.IsActive = dialog.ServerActivation;
                        }
                    }
                    else
                    {
                        var oldSubtitleServer = await db.SubtitleServers
                            .Where(x => x.Title == server.Title && x.FilePath == server.FilePath).FirstOrDefaultAsync();
                        oldSubtitleServer.Title = dialog.ServerTitle;
                        oldSubtitleServer.FilePath = dialog.ServerUrl;
                        oldSubtitleServer.IsActive = dialog.ServerActivation;
                    }

                    await db.SaveChangesAsync();

                    InfoBarSeverity = InfoBarSeverity.Success;
                    InfoBarTitle = "媒体库修改成功";
                    InfoBarIsOpen = true;
                    await OnPageLoaded();
                    Search(tempQuery);
                };

                await dialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            InfoBarSeverity = InfoBarSeverity.Error;
            InfoBarTitle = ex.Message;
            InfoBarIsOpen = true;
            Logger?.Error(ex, "ServerViewModel: OnUpdateServer");
        }
    }

    [RelayCommand]
    private async Task OnDeleteServer(object dataContext)
    {
        try
        {
            if (dataContext != null)
            {
                var server = dataContext as BaseServerTable;
                using var db = new AppDbContext();
                if (SegmentedSelectedIndex == 0)
                {
                    var delete = await db.MediaServers.Where(x =>
                            x.Title == server.Title && x.FilePath == server.FilePath &&
                            x.ServerType == server.ServerType)
                        .FirstOrDefaultAsync();
                    if (delete != null)
                    {
                        db.MediaServers.Remove(delete);
                    }
                }
                else
                {
                    var delete = await db.SubtitleServers.Where(x =>
                            x.Title == server.Title && x.FilePath == server.FilePath &&
                            x.ServerType == ServerType.Subtitle)
                        .FirstOrDefaultAsync();
                    if (delete != null)
                    {
                        db.SubtitleServers.Remove(delete);
                    }
                }

                await db.SaveChangesAsync();
                InfoBarSeverity = InfoBarSeverity.Success;
                InfoBarTitle = "删除成功";
                InfoBarIsOpen = true;
                await OnPageLoaded();
                Search(tempQuery);
            }
        }
        catch (Exception ex)
        {
            InfoBarSeverity = InfoBarSeverity.Error;
            InfoBarTitle = ex.Message;
            InfoBarIsOpen = true;
            Logger?.Error(ex, "ServerViewModel: OnDeleteServer");
        }
    }

    [RelayCommand]
    private async Task OnLoadPredefinedServer()
    {
        try
        {
            var dialog = new LoadPredefinedContentDialog();
            dialog.ThemeService = themeService;
            dialog.PrimaryButtonClick += async (s, e) =>
            {
                await OnDeleteAllServer();

                var filePath = Constants.DEFAULT_MEDIA_SERVER_PATH;

                if (SegmentedSelectedIndex != 0)
                {
                    filePath = Constants.DEFAULT_SUBTITLE_SERVER_PATH;
                }

                using var streamReader = File.OpenText(await PathHelper.GetFilePath(filePath));
                var json = await streamReader.ReadToEndAsync();
                using var db = new AppDbContext();
                if (SegmentedSelectedIndex == 0)
                {
                    var content = JsonConvert.DeserializeObject<List<MediaServerTable>>(json);

                    if (content is not null)
                    {
                        await db.MediaServers.AddRangeAsync(content);
                    }
                }
                else
                {
                    var content = JsonConvert.DeserializeObject<List<SubtitleServerTable>>(json);
                    if (content is not null)
                    {
                        await db.SubtitleServers.AddRangeAsync(content);
                    }
                }

                await db.SaveChangesAsync();
                InfoBarTitle = "Predefined Servers Loaded Successfully";
                InfoBarSeverity = InfoBarSeverity.Success;
                InfoBarIsOpen = true;
                await OnPageLoaded();
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            InfoBarSeverity = InfoBarSeverity.Error;
            InfoBarTitle = ex.Message;
            InfoBarIsOpen = true;
            Logger?.Error(ex, "ServerViewModel: OnLoadPredefinedServers");
        }
    }

    [RelayCommand]
    private async Task OnDeleteAllServer()
    {
        try
        {
            using var db = new AppDbContext();
            if (SegmentedSelectedIndex == 0)
            {
                await db.DeleteAndRecreateServerTables("MediaServer");
            }
            else
            {
                await db.DeleteAndRecreateServerTables("SubtitleServer");
            }

            InfoBarSeverity = InfoBarSeverity.Success;
            InfoBarTitle = "All Servers Deleted Successfully";
            InfoBarIsOpen = true;
            await OnPageLoaded();
            Search(tempQuery);
        }
        catch (Exception ex)
        {
            InfoBarSeverity = InfoBarSeverity.Error;
            InfoBarTitle = ex.Message;
            InfoBarIsOpen = true;
            Logger?.Error(ex, "ServerViewModel: OnDeleteAllServer");
        }
    }

    [RelayCommand]
    private async Task OnIndexingServer(object dataContext)
    {
        IsActive = true;
        InfoBarIsOpen = false;

        await Task.Run(() =>
        {
            dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    if (dataContext != null)
                    {
                        var server = dataContext as BaseServerTable;
                        // TODO: 检查是否存在
                        if (Directory.Exists(server.FilePath))
                        {
                            using var db = new AppDbContext();
                            // 根据目录索引所有媒体文件
                            var filePath = Directory.GetFiles(server.FilePath, "*", SearchOption.AllDirectories)
                                .Where(f => Constants.FileExtensions.Contains(Path.GetExtension(f),
                                    StringComparer.OrdinalIgnoreCase));
                            var namingOptions = new NamingOptions();
                            var result =
                                await VideoFileHelper.ParseFilePathAsync(filePath, server.ServerType, namingOptions);
                            if (result != null)
                            {
                                var type = await db.Storages
                                    .Where(p => p.FilePath == server.FilePath)
                                    .Select(p => p.ServerType)
                                    .FirstOrDefaultAsync();
                                foreach (var item in result)
                                {
                                }
                            }
                        }
                        else
                        {
                            InfoBarSeverity = InfoBarSeverity.Error;
                            InfoBarTitle = $"路径 {server.FilePath} 无效";
                            InfoBarIsOpen = true;
                            IsActive = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    InfoBarSeverity = InfoBarSeverity.Error;
                    InfoBarTitle = ex.Message;
                    InfoBarIsOpen = true;
                    IsActive = false;
                    Logger?.Error(ex, "ServerViewModel: OnIndexingServer");
                }
            });
        });

        IsActive = false;
    }

    [RelayCommand]
    private async Task OnBeachIndexingServer()
    {
        if (ServerList != null && ServerList.Any())
        {
            foreach (var item in ServerList)
            {
                await OnIndexingServer(item);
            }
        }
    }

    public void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        tempQuery = sender.Text;
        Search(sender.Text);
    }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        tempQuery = sender.Text;
        Search(sender.Text);
    }

    private void Search(string query)
    {
        try
        {
            if (ServerList != null && ServerList.Any())
            {
                ServerListACV.Filter = _ => true;
                ServerListACV.Filter = item =>
                {
                    var baseServer = (BaseServerTable)item;
                    var name = baseServer.Title ?? "";
                    var tName = baseServer.FilePath ?? "";
                    return name.Contains(query, StringComparison.OrdinalIgnoreCase)
                           || tName.Contains(query, StringComparison.OrdinalIgnoreCase);
                };
            }
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "ServerViewModel:Search");
        }
    }
}