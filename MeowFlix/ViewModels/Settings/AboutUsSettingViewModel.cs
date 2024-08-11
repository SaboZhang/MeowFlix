namespace MeowFlix.ViewModels;
public partial class AboutUsSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public string appInfo = $"{App.Current.AppName} v{App.Current.AppVersion}";

    [ObservableProperty]
    public string appCopyright = $"Copyright © {DateTime.Now.Year} {App.Current.AppName}";
}