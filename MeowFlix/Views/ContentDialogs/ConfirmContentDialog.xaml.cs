using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MeowFlix.Views
{
    public sealed partial class ConfirmContentDialog : ContentDialog
    {
        public string ConfirmText { get; set; }

        public bool IsCheckBoxChecked { get; set; }

        public BaseViewModel ViewModel { get; set; }
        public IThemeService ThemeService { get; set; }
        public ConfirmContentDialog()
        {
            this.InitializeComponent();
            XamlRoot = App.currentWindow.Content.XamlRoot;
            Loaded += ConfirmContentDialog_Loaded;
        }

        private void ConfirmContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            RequestedTheme = ThemeService.GetElementTheme();
            
        } 


    }
}
