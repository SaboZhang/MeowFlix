using MeowFlix.Database.Tables;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Windows.Input;
using Windows.System;

namespace MeowFlix.Views;
public sealed partial class DetailsUserControl : UserControl
{
    public event EventHandler<RoutedEventArgs> Click;
    public event EventHandler<RoutedEventArgs> DoubleClick;

    public CornerRadius SettingsCardCornerRadius
    {
        get => (CornerRadius)GetValue(SettingsCardCornerRadiusProperty);
        set => SetValue(SettingsCardCornerRadiusProperty, value);
    }
    public BaseMediaTable BaseMedia
    {
        get => (BaseMediaTable)GetValue(BaseMediaProperty);
        set => SetValue(BaseMediaProperty, value);
    }

    public BaseViewModel ViewModel
    {
        get => (BaseViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public ObservableCollection<MediaActorTable> Actors
    {
        get { return (ObservableCollection<MediaActorTable>)GetValue(ActorsProperty); }
        set { SetValue(ActorsProperty, value); }
    }

    public ICommand SettingsCardCommand
    {
        get => (ICommand)GetValue(SettingsCardCommandProperty);
        set => SetValue(SettingsCardCommandProperty, value);
    }

    public ICommand SettingsCardDoubleClickCommand
    {
        get => (ICommand)GetValue(SettingsCardDoubleClickCommandProperty);
        set => SetValue(SettingsCardDoubleClickCommandProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string FilePath
    {
        get => (string)GetValue(ServerProperty);
        set => SetValue(ServerProperty, value);
    }

    public string DateTime
    {
        get => (string)GetValue(DateTimeProperty);
        set => SetValue(DateTimeProperty, value);
    }

    public string FileSize
    {
        get => (string)GetValue(FileSizeProperty);
        set => SetValue(FileSizeProperty, value);
    }

    public string Year
    {
        get => (string)GetValue(YearProperty);
        set => SetValue(YearProperty, value);
    }

    public IconElement HeaderIcon
    {
        get => (IconElement)GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    public IconElement ActionIcon
    {
        get => (IconElement)GetValue(ActionIconProperty);
        set => SetValue(ActionIconProperty, value);
    }

    public BitmapImage Poster
    {
        get => (BitmapImage)GetValue(PosterProperty);
        set => SetValue(PosterProperty, value);
    }

    public static readonly DependencyProperty BaseMediaProperty =
        DependencyProperty.Register("BaseMedia", typeof(BaseMediaTable), typeof(DetailsUserControl), new PropertyMetadata(default(BaseMediaTable)));

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register("ViewModel", typeof(BaseViewModel), typeof(DetailsUserControl), new PropertyMetadata(default(BaseViewModel)));

    public static readonly DependencyProperty ActorsProperty =
        DependencyProperty.Register("Actors", typeof(ObservableCollection<MediaActorTable>), typeof(ObservableCollection<MediaActorTable>), new PropertyMetadata(default(ObservableCollection<MediaActorTable>)));

    public static readonly DependencyProperty SettingsCardCommandProperty =
       DependencyProperty.Register("SettingsCardCommand", typeof(ICommand), typeof(DetailsUserControl), new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty SettingsCardDoubleClickCommandProperty =
       DependencyProperty.Register("SettingsCardDoubleClickCommand", typeof(ICommand), typeof(DetailsUserControl), new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(DetailsUserControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ServerProperty =
        DependencyProperty.Register("Server", typeof(string), typeof(DetailsUserControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty DateTimeProperty =
        DependencyProperty.Register("DateTime", typeof(string), typeof(DetailsUserControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty FileSizeProperty =
        DependencyProperty.Register("FileSize", typeof(string), typeof(DetailsUserControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty YearProperty =
        DependencyProperty.Register("Year", typeof(string), typeof(DetailsUserControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register("HeaderIcon", typeof(IconElement), typeof(DetailsUserControl), new PropertyMetadata(default(IconElement)));

    public static readonly DependencyProperty ActionIconProperty =
        DependencyProperty.Register("ActionIcon", typeof(IconElement), typeof(DetailsUserControl), new PropertyMetadata(default(IconElement)));

    public static readonly DependencyProperty PosterProperty =
        DependencyProperty.Register("Poster", typeof(BitmapImage), typeof(DetailsUserControl), new PropertyMetadata(default(BitmapImage)));

    public static readonly DependencyProperty SettingsCardCornerRadiusProperty =
        DependencyProperty.Register("SettingsCardCornerRadius", typeof(CornerRadius), typeof(DetailsUserControl), new PropertyMetadata(default(CornerRadius)));

    public IJsonNavigationViewService JsonNavigationViewService { get; set; }

    public MediaDetailsViewModel DetailsViewModel { get; }



    public DetailsUserControl()
    {
        this.InitializeComponent();
        DetailsViewModel = App.GetService<MediaDetailsViewModel>();

        if (Poster == null)
        {
            Poster = new BitmapImage(new Uri("ms-appx:///Assets/Cover/Poster.png"));
        }

        if (ActionIcon == null)
        {
            ActionIcon = new FontIcon { Glyph = "\ue974" };
        }
        Loaded += OnLoaded;

    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Actors != null)
        {
            System.Diagnostics.Debug.WriteLine($"Actors count: {Actors.Count}");
        }
    }

    private void SettingsCard_Click(object sender, RoutedEventArgs e)
    {
        Click?.Invoke(sender, e);
    }

    private void SettingsCard_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        DoubleClick?.Invoke(sender, e);
    }

    private async void ServerHyperLink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await Launcher.LaunchUriAsync(new Uri(FilePath));
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "ItemUserControl: Navigate To Uri for Server");
        }
    }

    /*private void HandleMouseEnter(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        overlay.Visibility = Visibility.Visible;
    }

    private void HandleMouseExit(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        overlay.Visibility = Visibility.Collapsed;
    }*/

    /*private void GoToDetails_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        ViewModel.GoToDetailsCommand.Execute((sender as Border).Tag);
    }*/

}
