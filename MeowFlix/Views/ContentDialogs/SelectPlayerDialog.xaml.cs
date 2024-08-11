// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MeowFlix.Views.ContentDialogs;

public sealed partial class SelectPlayerDialog : ContentDialog
{
    public IThemeService ThemeService { get; set; }

    public string VideoPath { get; set; }

    public MediaViewModel ViewModel { get; set; }
    public SelectPlayerDialog()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<MediaViewModel>();
        XamlRoot = App.currentWindow.Content.XamlRoot;
        Loaded += SelectPlayerDialog_Loaded;
    }

    private void SelectPlayerDialog_Loaded(object sender, RoutedEventArgs e)
    {
        RequestedTheme = ElementTheme.Dark;
    }

    private void PlayerButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.VideoPath = VideoPath;
        ViewModel.SelectPlayerCommand.Execute(sender);
    }
}
