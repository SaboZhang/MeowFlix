// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MeowFlix.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AuthDialog : ContentDialog
{
    public string Password;
    public IThemeService ThemeService { get; set; }
    public AuthDialog()
    {
        this.InitializeComponent();
        XamlRoot = App.currentWindow.Content.XamlRoot;
        Loaded += ChangeAuthContentDialog_Loaded;
    }

    private void ChangeAuthContentDialog_Loaded(object sender, RoutedEventArgs e)
    {
        RequestedTheme = ThemeService.GetElementTheme();
    }
}
