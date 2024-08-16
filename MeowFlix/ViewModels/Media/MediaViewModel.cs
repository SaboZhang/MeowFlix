using CommunityToolkit.WinUI.Controls;
using CommunityToolkit.WinUI.UI;
using HtmlAgilityPack;
using MeowFlix.Database;
using MeowFlix.Database.Tables;
using MeowFlix.Views.ContentDialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json;

namespace MeowFlix.ViewModels;
public partial class MediaViewModel : BaseViewModel, ITitleBarAutoSuggestBoxAware
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    private int segmentedSelectedIndex = 0;

    [ObservableProperty]
    public ObservableCollection<SegmentedItem> segmentedItems;

    [ObservableProperty]
    public int segmentedItemSelectedIndex = 0;

    [ObservableProperty]
    public object segmentedItemSelectedItem = null;

    [ObservableProperty]
    public bool isServerStatusOpen;

    [ObservableProperty]
    public int progressBarValue;

    [ObservableProperty]
    public int progressBarMaxValue;

    [ObservableProperty]
    public bool progressBarShowError;

    public string channel = string.Empty;

    public object ReceivedParameter { get; set; }

    private DispatcherTimer dispatcherTimer;
    private ServerType PageType;
    private int totalServerCount = 0;
    private List<ExceptionModel> exceptions;
    public IJsonNavigationViewService JsonNavigationViewService;
    public IThemeService themeService;
    public MediaViewModel(IJsonNavigationViewService jsonNavigationViewService, IThemeService themeService)
    {
        JsonNavigationViewService = jsonNavigationViewService;
        this.themeService = themeService;
        Task.Run(() =>
        {
            dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    using var db = new AppDbContext();
                    if (!db.Chanels.Any())
                    {
                        using var streamReader = File.OpenText(await PathHelper.GetFilePath(Constants.DEFAULT_CHANNEL));
                        var json = await streamReader.ReadToEndAsync();
                        var content = JsonConvert.DeserializeObject<List<ChannelTable>>(json);
                        if (content is not null)
                        {
                            await db.Chanels.AddRangeAsync(content);
                        }
                        await db.SaveChangesAsync();
                    }
                    var segments = db.Chanels.Where(x => x.IsActive)
                        .Select(x => new SegmentedItem { Content = x.Title, Icon = new FontIcon { Glyph = x.FilePath } });

                    SegmentedItems = new(segments);

                    var defaultTokenItem = SegmentedItems.FirstOrDefault(x => x.Content.ToString().Contains("全部", StringComparison.OrdinalIgnoreCase));
                    SegmentedItemSelectedIndex = SegmentedItems.IndexOf(defaultTokenItem);
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, "MediaViewModel:Ctor");
                }
            });
        });
    }

    public override async void OnPageLoaded(object param)
    {
        IsActive = true;

        await Task.Run(() =>
        {
            dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    PageType = MediaPage.Instance.PageType;
                    using var db = new AppDbContext();
                    if (!db.Storages.Any())
                    {
                        IsStatusOpen = true;
                        StatusTitle = "影视文件未找到";
                        StatusMessage = "未找到影视文件, 请添加媒体库";
                        StatusSeverity = InfoBarSeverity.Warning;
                        IsServerStatusOpen = false;
                        var dialog = new GoToServerContentDialog();
                        dialog.ThemeService = themeService;
                        dialog.JsonNavigationViewService = JsonNavigationViewService;
                        await dialog.ShowAsync();
                    }
                    else
                    {
                        await LoadLocalMedia();
                    }
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, "MediaViewModel:OnPageLoaded");
                }
            });
        });

        IsActive = false;
    }

    public override async void OnRefresh()
    {
        dispatcherTimer?.Stop();
        dispatcherTimer = null;
        await LoadLocalMedia();
    }

    [RelayCommand]
    private async Task OnSegmentedSelectionChanged()
    {
        IsActive = true;

        var index = SegmentedItemSelectedIndex;

        if (index >= 0 && index < SegmentedItems.Count)
        {
            var item = SegmentedItems[index];
            var selectedValue = item.Content.ToString();

            channel = selectedValue;

            if (selectedValue == "全部")
            {
                channel = "all";
            }

            await LoadLocalMedia();
            IsActive = false;

        }
    }

    public override void NavigateToDetails(object sender)
    {
        base.NavigateToDetails(sender);

        var media = new BaseMediaTable
        {
            FilePath = descriptionText,
            Title = headerText,
            ServerType = PageType
        };

        JsonNavigationViewService.NavigateTo(typeof(MediaDetailPage), media, false, new DrillInNavigationTransitionInfo());

    }

    [RelayCommand]
    private async Task OnServerStatus()
    {
        var dialog = new ServerErrorsContentDialog();
        dialog.ThemeService = themeService;
        dialog.Exceptions = new(exceptions);

        await dialog.ShowAsync();
    }

    private async Task LoadLocalMedia()
    {
        await Task.Run(() =>
        {
            dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    IsActive = true;
                    IsStatusOpen = true;
                    StatusSeverity = InfoBarSeverity.Informational;
                    StatusTitle = "加载媒体文件中，请稍后！";

                    using var db = new AppDbContext();
                    List<BaseMediaTable> media = null;
                    if (channel.Equals(Constants.ENCRYPT_CHANNEL))
                    {
                        var result = await CredentialHelper.RequestWindowsPIN("请进行隐私验证");
                        OnAuthPrimaryButton(result);
                        return;
                    }
                    switch (PageType)
                    {
                        case ServerType.Movies when channel.Equals("all"):
                            media = new(await db.Movies.Where(x => x.FilePath != null && !x.Channel.Equals(Constants.ENCRYPT_CHANNEL)).ToListAsync());
                            break;
                        case ServerType.Movies:
                            media = new(await db.Movies.Where(x => x.FilePath != null && x.Channel == channel && !x.Channel.Equals(Constants.ENCRYPT_CHANNEL)).ToListAsync());
                            break;
                        case ServerType.Series when channel.Equals("all"):
                            media = new(await db.Series.Where(x => x.FilePath != null && !x.Channel.Equals(Constants.ENCRYPT_CHANNEL)).ToListAsync());
                            break;
                        case ServerType.Series:
                            media = new(await db.Series.Where(x => x.FilePath != null && x.Channel == channel && !x.Channel.Equals(Constants.ENCRYPT_CHANNEL)).ToListAsync());
                            break;
                    }

                    if (media != null && media.Count != 0)
                    {
                        DataList = [];
                        DataListACV = new AdvancedCollectionView(DataList, true);

                        using (DataListACV.DeferRefresh())
                        {
                            DataList.AddRange(media);
                        }

                        DataListACV.SortDescriptions.Add(new SortDescription("Title", SortDirection.Ascending));
                    }

                    IsActive = false;
                    StatusSeverity = InfoBarSeverity.Success;
                    StatusTitle = $"共 ({DataList?.Count}) 个筛选结果";
                    StatusMessage = "";
                }
                catch (Exception ex)
                {
                    IsActive = false;
                    StatusSeverity = InfoBarSeverity.Error;
                    StatusTitle = "Error";
                    StatusMessage = ex.Message;
                    Logger?.Error(ex, "MediaViewModel: LoadLocalMedia");
                }
            });
        }).ContinueWith(x =>
        {
            AutoHideStatusInfoBar();
        });
    }

    private async void OnAuthPrimaryButton(bool auth)
    {
        try
        {
            IsActive = true;
            IsStatusOpen = true;
            StatusSeverity = InfoBarSeverity.Informational;
            StatusTitle = "加载媒体文件中，请稍后！";
            if (auth)
            {
                var db = new AppDbContext();
                List<BaseMediaTable> media = null;
                switch (PageType)
                {
                    case ServerType.Movies:
                        media = new(await db.Movies.Where(x => x.FilePath != null && x.Channel.Equals(Constants.ENCRYPT_CHANNEL)).ToListAsync());
                        break;
                    case ServerType.Series:
                        media = new(await db.Series.Where(x => x.FilePath != null && x.Channel.Equals(Constants.ENCRYPT_CHANNEL)).ToListAsync());
                        break;
                }

                DataList = [];
                DataListACV = new AdvancedCollectionView(DataList, true);

                using (DataListACV.DeferRefresh())
                {
                    DataList.AddRange(media);
                }

                DataListACV.SortDescriptions.Add(new SortDescription("Title", SortDirection.Ascending));
            }
            IsActive = false;
            StatusSeverity = InfoBarSeverity.Success;
            StatusTitle = $"共 ({DataList?.Count}) 个筛选结果";
            StatusMessage = "";
        }
        catch (Exception ex)
        {
            IsActive = false;
            StatusSeverity = InfoBarSeverity.Error;
            StatusTitle = "Error";
            StatusMessage = ex.Message;
            Logger?.Error(ex, "MediaViewModel: OnAuthPrimaryButton");
        }
    }

    public override async void OnRemoveFile(object baseMedia)
    {
        if (baseMedia == null)
        {
            return;
        }
        var server = baseMedia as BaseMediaTable;
        ReceivedParameter = server;
        var dialog = new ConfirmContentDialog
        {
            ThemeService = themeService,
            ConfirmText = $"确认删除 {server.Title} 吗",
            IsCheckBoxChecked = false
        };
        dialog.PrimaryButtonClick += OnDeleteFilePrimaryButton;

        await dialog.ShowAsync();


    }

    private async void OnDeleteFilePrimaryButton(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        IsActive = true;
        try
        {
            if (ReceivedParameter != null)
            {
                var server = ReceivedParameter as BaseMediaTable;
                var dialog = sender as ConfirmContentDialog;
                using var db = new AppDbContext();
                switch (server.ServerType)
                {
                    case ServerType.Movies:
                        var deleteMovie = await db.Movies
                            .Where(x => x.Title == server.Title && x.FilePath == server.FilePath && x.ServerType == server.ServerType)
                            .Include(x => x.Actors)
                            .Include(x => x.StreamDetails)
                            .FirstOrDefaultAsync();
                        if (deleteMovie != null)
                        {
                            if (dialog.IsCheckBoxChecked)
                            {
                                File.Delete(server.FilePath);
                            }
                            db.Movies.Remove(deleteMovie);
                        }
                        break;
                    case ServerType.Series:
                        var deleteSeries = await db.Series
                            .Where(x => x.Title == server.Title && x.FilePath == server.FilePath && x.ServerType == server.ServerType)
                            .FirstOrDefaultAsync();
                        if (deleteSeries != null)
                        {
                            if (dialog.IsCheckBoxChecked)
                            {
                                File.Delete(server.FilePath);
                            }
                            db.Series.Remove(deleteSeries);
                        }
                        break;
                }

                await db.SaveChangesAsync();
                await LoadLocalMedia();

                Growl.Success("删除成功");

            }
        }
        catch (Exception ex)
        {
            IsActive = false;
            Growl.Error("删除失败");
            Logger?.Error(ex, "ServerViewModel: OnDeleteServer");
        }
        IsActive = false;
    }

    public async Task GetDonyayeSerialServerDetails(string content, string server)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var rows = doc.DocumentNode.SelectNodes("//table[@class='table']/tbody/tr");
            var ignoreLinks = new List<string> { "../", "Home", "DonyayeSerial", "series", "movie" };
            using var db = new AppDbContext();
            if (rows == null)
            {
                return;
            }
            foreach (var row in rows)
            {
                var nameNode = row.SelectSingleNode("./td[@class='n']/a/code");
                var dateNode = row.SelectSingleNode("./td[@class='m']/code");
                var linkNode = row.SelectSingleNode("./td[@class='n']/a");
                var sizeNode = row.SelectSingleNode("./td[@class='s']");
                if (linkNode != null && !ignoreLinks.Contains(linkNode.Attributes["href"].Value))
                {
                    var title = nameNode?.InnerText?.Trim();
                    var date = dateNode?.InnerText?.Trim();
                    var serverUrl = $"{server}{linkNode?.Attributes["href"]?.Value?.Trim()}";
                    var size = sizeNode?.InnerText?.Trim();
                    /*switch (PageType)
                    {
                        case ServerType.Anime:
                            await db.Animes.AddAsync(new AnimeTable(title, serverUrl, date, size, ServerType.Anime));
                            break;
                        case ServerType.Movies:
                            await db.Movies.AddAsync(new MovieTable(title, serverUrl, date, size, ServerType.Movies));
                            break;
                        case ServerType.Series:
                            await db.Series.AddAsync(new SeriesTable(title, serverUrl, date, size, ServerType.Series));
                            break;
                    }*/
                }
            }
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            IsActive = false;
            StatusTitle = "Error";
            StatusMessage = ex.Message;
            StatusSeverity = InfoBarSeverity.Error;
            if (exceptions.Any())
            {
                IsServerStatusOpen = true;
            }
            Logger?.Error(ex, "MediViewModel: GetDonyayeSerialServerDetails");
        }
    }

    public async Task GetAllServerDetails(string content, string server)
    {
        try
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            var nodes = doc?.DocumentNode?.SelectNodes("//a[@href]");

            if (nodes != null)
            {
                using var db = new AppDbContext();

                foreach (var node in nodes)
                {
                    var href = node?.GetAttributeValue("href", "");

                    var title = node?.InnerText;
                    var date = node?.NextSibling?.InnerText?.Trim();
                    if (string.IsNullOrEmpty(date))
                    {
                        date = node?.PreviousSibling?.InnerText?.Trim();
                    }

                    var dateAndSize = date?.Split("  ");
                    if (dateAndSize?.Length > 1)
                    {
                        date = dateAndSize[0];
                    }

                    if (ContinueIfWrongData(title, href, server, null))
                    {
                        continue;
                    }

                    var finalServer = FixUriDuplication(server, href);
                    if (finalServer.Contains("directadmin.com", StringComparison.OrdinalIgnoreCase) ||
                        finalServer.Contains("acemovies") && (title.Equals("Home") ||
                        title.Equals("dl") || title.Equals("English") || title.Equals("Series") ||
                        title.Equals("Movie") || title.Contains("Parent Directory")))
                    {
                        continue;
                    }

                    /*switch (PageType)
                    {
                        case ServerType.Anime:
                            await db.Animes.AddAsync(new AnimeTable(FixTitle(title), finalServer, date, null, ServerType.Anime));
                            break;
                        case ServerType.Movies:
                            await db.Movies.AddAsync(new MovieTable(FixTitle(title), finalServer, date, null, ServerType.Movies));
                            break;
                        case ServerType.Series:
                            await db.Series.AddAsync(new SeriesTable(FixTitle(title), finalServer, date, null, ServerType.Series));
                            break;
                    }*/
                }
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            IsActive = false;
            StatusTitle = "Error";
            StatusMessage = ex.Message;
            StatusSeverity = InfoBarSeverity.Error;
            if (exceptions.Any())
            {
                IsServerStatusOpen = true;
            }
            Logger?.Error(ex, "MediViewModel: GetAllServerDetails");
        }
    }

    public async Task GetServerDetails(string content, string server, ServerType serverType)
    {
        await Task.Run(() =>
        {
            dispatcherQueue.TryEnqueue(async () =>
            {
                if (server.Contains("DonyayeSerial", StringComparison.OrdinalIgnoreCase))
                {
                    await GetDonyayeSerialServerDetails(content, server);
                }
                else
                {
                    await GetAllServerDetails(content, server);
                }
            });
        });
    }

    private void AutoHideStatusInfoBar()
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            dispatcherTimer = new DispatcherTimer();
            TimeSpan timeSpan = new TimeSpan(0, 0, 20);
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (s, e) =>
            {
                dispatcherTimer?.Stop();
                dispatcherTimer = null;
                StatusMessage = "";
                StatusTitle = "";
                StatusSeverity = InfoBarSeverity.Informational;
                IsStatusOpen = false;
            };
            dispatcherTimer.Interval = timeSpan;
            dispatcherTimer.Start();
        });
    }

    public void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Search(sender, args);
    }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        Search(sender, null);
    }

    private async void Search(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        try
        {
            if (DataList != null)
            {
                using var db = new AppDbContext();
                if (args != null)
                {
                    AutoSuggestBoxHelper.LoadSuggestions(sender, args, DataList.Select(x => x.Title).ToList());
                }

                List<BaseMediaTable> media = new();

                switch (PageType)
                {
                    case ServerType.Anime:
                        media = new(await db.Animes.Where(x => x.Title.ToLower().Contains(sender.Text.ToLower()) || x.FilePath.ToLower().Contains(sender.Text.ToLower())).ToListAsync());
                        break;
                    case ServerType.Movies:
                        media = new(await db.Movies.Where(x => x.Title.ToLower().Contains(sender.Text.ToLower()) || x.FilePath.ToLower().Contains(sender.Text.ToLower())).ToListAsync());
                        break;
                    case ServerType.Series:
                        media = new(await db.Series.Where(x => x.Title.ToLower().Contains(sender.Text.ToLower()) || x.FilePath.ToLower().Contains(sender.Text.ToLower())).ToListAsync());
                        break;
                }

                DataList = new(media);
                DataListACV = new AdvancedCollectionView(DataList, true);
            }
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "MediaViewModel: Search");
            StatusTitle = "Error";
            StatusMessage = ex.Message;
            StatusSeverity = InfoBarSeverity.Error;
        }
    }
}
