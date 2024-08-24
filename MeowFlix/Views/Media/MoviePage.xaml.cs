namespace MeowFlix.Views;
public sealed partial class MoviePage : Page
{
    public ServerType PageType { get; set; }
    public static MoviePage Instance { get; set; }
    public MediaViewModel ViewModel { get; }
    public MoviePage()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<MediaViewModel>();
        this.InitializeComponent();
        Instance = this;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var pageType = (e.Parameter as DataItem).Parameter?.ToString();
        this.PageType = GeneralHelper.GetEnum<ServerType>(pageType);
    }

    private void ItemUserControl_Loading(FrameworkElement sender, object args)
    {
        var item = sender as ItemUserControl;
        item.ViewModel = ViewModel;
        item.SettingsCardCommand = ViewModel.SettingsCardCommand;
        item.SettingsCardDoubleClickCommand = ViewModel.SettingsCardDoubleClickCommand;
        var conv = new MediaHeaderIconConverter();
        var headerIcon = conv.Convert(PageType, null, null, null);
        if (headerIcon != null)
        {
            item.HeaderIcon = (BitmapIcon)headerIcon;
        }
    }
}
