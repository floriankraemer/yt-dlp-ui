using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using YtDlpUi.UI.Services;
using YtDlpUi.UI.Views;

namespace YtDlpUi.UI;

public sealed partial class App : Application
{
    public static AppServices Services { get; } = new();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.CreateMainViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
