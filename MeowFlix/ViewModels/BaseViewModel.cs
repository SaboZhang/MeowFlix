using CommunityToolkit.WinUI.UI;
using MeowFlix.Database.Tables;
using MeowFlix.Tools;
using MeowFlix.Views.ContentDialogs;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace MeowFlix.ViewModels;
public partial class BaseViewModel : ObservableRecipient
{
    [ObservableProperty]
    public ObservableCollection<BaseMediaTable> dataList;

    [ObservableProperty]
    public AdvancedCollectionView dataListACV;

    [ObservableProperty]
    public bool isStatusOpen;

    [ObservableProperty]
    public string statusMessage;

    [ObservableProperty]
    public string statusTitle;

    [ObservableProperty]
    public InfoBarSeverity statusSeverity;

    public BaseMediaTable rootMedia = null;

    public string headerText = string.Empty;
    public string descriptionText = string.Empty;

    [ObservableProperty]
    public string videoPath;

    public string videoUrl { get; set; }



    #region MenuFlyoutItems
    [RelayCommand]
    private async Task OnOpenWebDirectory(object item)
    {
        try
        {
            var menuItem = item as MenuFlyoutItem;
            var media = menuItem.DataContext as BaseMediaTable;
            var filePath = media.FilePath?.ToString();
            if (menuItem.Text.Contains("File"))
            {
                await Launcher.LaunchUriAsync(new Uri(filePath));
            }
            else
            {
                if (Constants.FileExtensions.Any(filePath.Contains))
                {
                    var fileName = Path.GetFileName(filePath);
                    await Launcher.LaunchUriAsync(new Uri(filePath.Replace(fileName, "")));
                }
                else
                {
                    await Launcher.LaunchUriAsync(new Uri(filePath));
                }
            }
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "BaseViewModel: OnOpenWebDirectory");
        }
    }

    [RelayCommand]
    private void OnCopy(object baseMedia)
    {
        var media = baseMedia as BaseMediaTable;
        var server = media.FilePath?.ToString();
        var package = new DataPackage();
        package.SetText(server);
        Clipboard.SetContent(package);
    }

    [RelayCommand]
    private void OnCopyAll()
    {
        if (DataList != null)
        {
            var package = new DataPackage();
            StringBuilder urls = new StringBuilder();
            foreach (var item in DataList)
            {
                urls.AppendLine(item.FilePath?.ToString());
            }
            package.SetText(urls?.ToString());
            Clipboard.SetContent(package);
        }
    }

    #endregion

    [RelayCommand]
    protected virtual async Task OnPlay(object baseMedia)
    {
        var playerPath = GetPlayerPath();
        var media = baseMedia as BaseMediaTable;
        var filePath = media.FilePath?.ToString();
        if (!string.IsNullOrEmpty(playerPath))
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Growl.Error("未找到媒体文件");
            }
            LaunchPlayer(playerPath, filePath);
        }
        else if (!string.IsNullOrEmpty(filePath))
        {
            var type = await LaunchPlayer(filePath);
            if (!type)
            {
                ToastWithAvatar.Instance.SendToast();
                var dialog = new SelectPlayerDialog
                {
                    VideoPath = filePath,
                    RequestedTheme = ElementTheme.Default
                };
                await dialog.ShowAsync();
            }
        }
    }

    [RelayCommand]
    protected virtual void OnOpenFolder(object baseMedia)
    {
        var media = baseMedia as BaseMediaTable;
        var filePath = media.FilePath?.ToString();
        if (!string.IsNullOrEmpty(filePath))
        {
            OpenFolderContainingFile(filePath);
        }
    }

    [RelayCommand]
    public virtual void OnRemoveFile(object baseMedia)
    {

    }

    [RelayCommand]
    public virtual void OnPageLoaded(object param)
    {

    }

    /// <summary>
    /// Use base for the default implementation, Use two properties headerText and descriptionText
    /// </summary>
    /// <param name="sender"></param>
    public virtual void NavigateToDetails(object sender)
    {
        var item = GetHeaderAndDescription(sender);

        headerText = item.Header;
        descriptionText = item.Description;
    }

    [RelayCommand]
    public virtual void OnSettingsCard(object sender)
    {
        if (!Settings.UseDoubleClickForNavigate)
        {
            NavigateToDetails(sender);
        }
    }

    [RelayCommand]
    public virtual void OnGoToDetails(object sender)
    {
        NavigateToDetails(sender);
    }

    [RelayCommand]
    public virtual void OnSettingsCardDoubleClick(object sender)
    {
        NavigateToDetails(sender);
    }

    [RelayCommand]
    public virtual void OnRefresh()
    {

    }

    [RelayCommand]
    public virtual void OnIMDBDetail()
    {

    }

    [RelayCommand]
    public virtual void OnSelectPlayer(object sender)
    {
        var param = sender as Button;

        switch (param.CommandParameter)
        {
            case "PotPlayer":
                LaunchPlayer(VideoPath, InstalledPlayer.PotPlayer);
                break;
            case "vlc":
                LaunchPlayer(VideoPath, InstalledPlayer.VLC);
                break;
            case "mpv":
                LaunchPlayer(VideoPath, InstalledPlayer.MPV);
                break;
        }
    }

    public (string Header, string Description) GetHeaderAndDescription(object sender)
    {
        var item = sender as ItemUserControl;
        var title = item?.Title?.Trim();
        var server = string.Empty;
        server = item?.FilePath?.ToString();

        return (Header: title, Description: server);
    }


}
