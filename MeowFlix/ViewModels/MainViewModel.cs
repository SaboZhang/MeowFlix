using Microsoft.UI.Xaml.Media;

namespace MeowFlix.ViewModels;
public partial class MainViewModel : ObservableObject
{
    public IJsonNavigationViewService JsonNavigationViewService;
    public MainViewModel(IJsonNavigationViewService jsonNavigationViewService, IThemeService themeService)
    {
        JsonNavigationViewService = jsonNavigationViewService;
        themeService.Initialize(App.currentWindow);
        themeService.ConfigBackdrop();
        themeService.ConfigElementTheme();
        themeService.ConfigFallbackColor();
    }
}