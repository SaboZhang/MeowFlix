﻿using MeowFlix.Database.Tables;

namespace MeowFlix.Views;

public sealed partial class MovieDetailPage : Page
{
    public static MovieDetailPage Instance { get; set; }
    public MediaDetailsViewModel ViewModel { get; }
    public MovieDetailPage()
    {
        ViewModel = App.GetService<MediaDetailsViewModel>();
        this.InitializeComponent();
        Instance = this;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var args = (BaseMediaTable)e.Parameter;
        ViewModel.rootMedia = args;
        ViewModel.BreadcrumbBarList?.Clear();
    }

    private void DetailsUserControl_Loading(FrameworkElement sender, object args)
    {
        var item = sender as ItemUserControl;
        item.ViewModel = ViewModel;
        /*item.SettingsCardCommand = ViewModel.SettingsCardCommand;
        item.SettingsCardDoubleClickCommand = ViewModel.SettingsCardDoubleClickCommand;*/
    }
}
