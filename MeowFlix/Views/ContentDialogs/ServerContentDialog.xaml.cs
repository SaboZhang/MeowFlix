namespace MeowFlix.Views.ContentDialogs;
public sealed partial class ServerContentDialog : ContentDialog
{
    public event EventHandler<RoutedEventArgs> Click;

    public string ServerTitle;

    public string ServerUrl;

    public object CmbServerTypeSelectedItem;
    
    public object CmbPathTypeSelectedItem;

    public bool ServerActivation;

    public string FloderPath;

    public bool IsPrivate;

    public string Password;

    public string WebPassWord;
    
    public string WebUserName;
    
    public string WebUrl;

    public ServerViewModel ViewModel { get; set; }
    public ServerContentDialog()
    {
        this.InitializeComponent();
        XamlRoot = App.currentWindow.Content.XamlRoot;
        
        Loaded += ServerContentDialog_Loaded;
    }

    private void ServerContentDialog_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            RequestedTheme = ViewModel.themeService.GetElementTheme();
        }
        if (CmbPathTypeSelectedItem != null)
        {
            CmbPathType.SelectedItem = CmbPathType.Items.FirstOrDefault(x => ((ComboBoxItem)x).Tag.ToString() == CmbPathTypeSelectedItem.ToString());
        }
        else
        {
            CmbPathType.SelectedIndex = 0;
        }

        if (CmbServerTypeSelectedItem != null)
        {
            CmbServerType.SelectedItem = CmbServerType.Items.FirstOrDefault(x => ((ComboBoxItem)x).Tag.ToString() == CmbServerTypeSelectedItem.ToString());
        }
        else
        {
            CmbServerType.SelectedIndex = 0;
        }
    }

    private void SelectLocation_Click(object sender, RoutedEventArgs e)
    {
        Click?.Invoke(this, e);
    }
}
