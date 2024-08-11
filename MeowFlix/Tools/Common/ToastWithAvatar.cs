using Microsoft.Windows.AppNotifications;

namespace MeowFlix.Common;

public class ToastWithAvatar : IScenario
{
    private static ToastWithAvatar _Instance;

    public static ToastWithAvatar Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new ToastWithAvatar();
            }
            return _Instance;
        }
        set { _Instance = value; }
    }
    public int ScenarioId { get; set; } = 1;
    public string ScenarioName { get; set; } = "系统提示";

    public void NotificationReceived(AppNotificationActivatedEventArgs notificationActivatedEventArgs)
    {
        var notification = NotificationHelper.GetNotificationForWithAvatar(ScenarioName, notificationActivatedEventArgs);
    }

    public bool SendToast()
    {
        string str = NotificationHelper.MakeScenarioIdToken(ScenarioId);
        return ScenarioHelper.SendToast(new string((ReadOnlySpan<char>)("<toast launch = \"action=ToastClick&amp;" +
                                                                        str +
                                                                        "\"><visual><binding template = \"ToastGeneric\"><image placement = \"appLogoOverride\" hint-crop=\"circle\" src = \"" +
                                                                        PathHelper.GetFullPathToAsset("Logo.png") +
                                                                        "\"/><text>" + ScenarioName + "</text><text>" +
                                                                        "网络路径无法使用默认播放器播放，请在弹窗选择拥有的播放器播放" +
                                                                        "</text></binding></visual></toast>")));
    }
}
