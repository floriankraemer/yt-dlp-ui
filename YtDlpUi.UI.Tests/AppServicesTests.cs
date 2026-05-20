using YtDlpUi.UI.Services;

namespace YtDlpUi.UI.Tests;

public sealed class AppServicesTests
{
    [Fact]
    public void CreateMainViewModel_ReturnsConfiguredInstance()
    {
        var services = new AppServices();
        var vm = services.CreateMainViewModel();
        Assert.NotNull(vm);
    }

    [Fact]
    public void CreateSettingsViewModel_ReturnsConfiguredInstance()
    {
        var services = new AppServices();
        var vm = services.CreateSettingsViewModel();
        Assert.NotNull(vm);
    }
}
