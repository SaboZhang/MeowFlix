using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
