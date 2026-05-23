// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using NSubstitute;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class YouTubeAccountViewModelTests : IDisposable
{
    private readonly string _root;
    private readonly IYouTubeAccountService _accountService;
    private readonly SettingsCoordinator _coordinator;
    private readonly IAppConfigStore _appConfigStore;

    public YouTubeAccountViewModelTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-youtube-vm", Guid.NewGuid().ToString("N"));
        _accountService = new YouTubeAccountService(_root, new YtDlpProcessRunner());
        var profiles = new ProfileStore(_root);
        _appConfigStore = new AppConfigStore(_root, profiles);
        _coordinator = new SettingsCoordinator(
            _appConfigStore,
            profiles,
            new YtDlpCommandBuilder(new YtDlpOptionCatalog(), new ExtraArgsTokenizer()),
            ViewModelTestHelpers.CreateValidator(),
            new BinaryLocator(_root),
            new JsRuntimeLocator(),
            _accountService);
    }

    [Fact]
    public async Task ImportAsync_Success_UpdatesSignedInState()
    {
        var vm = CreateViewModel();
        var source = Path.Combine(_root, "import.txt");
        Directory.CreateDirectory(_root);
        File.WriteAllText(source, "# Netscape HTTP Cookie File\n");

        await vm.ImportAsync(source);

        Assert.True(vm.IsSignedIn);
        Assert.Equal("Signed in", vm.Status);
        Assert.False(string.IsNullOrWhiteSpace(vm.CookiesPathDisplay));
    }

    [Fact]
    public async Task ImportAsync_Failure_SetsErrorMessage()
    {
        var vm = CreateViewModel();

        await vm.ImportAsync(Path.Combine(_root, "missing.txt"));

        Assert.False(vm.IsSignedIn);
        Assert.NotNull(vm.ErrorMessage);
    }

    [Fact]
    public async Task SignOutAsync_ClearsSignedInState()
    {
        var vm = CreateViewModel();
        var source = Path.Combine(_root, "import.txt");
        Directory.CreateDirectory(_root);
        File.WriteAllText(source, "# Netscape HTTP Cookie File\n");
        await vm.ImportAsync(source);

        await vm.SignOutAsync();

        Assert.False(vm.IsSignedIn);
        Assert.Equal("Not signed in", vm.Status);
    }

    [Fact]
    public async Task TestAsync_WhenNotSignedIn_SetsErrorMessage()
    {
        var vm = CreateViewModel();

        await vm.TestAsync();

        Assert.NotNull(vm.ErrorMessage);
    }

    [Fact]
    public async Task ImportAsync_WhenBusy_DoesNotReenter()
    {
        var account = Substitute.For<IYouTubeAccountService>();
        account.ImportAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                await Task.Delay(200, call.Arg<CancellationToken>());
                return new YouTubeImportResult(true, null, null);
            });

        var vm = new YouTubeAccountViewModel(account, _coordinator, _appConfigStore);
        var source = Path.Combine(_root, "import.txt");
        Directory.CreateDirectory(_root);
        File.WriteAllText(source, "# Netscape HTTP Cookie File\n");

        var first = vm.ImportAsync(source);
        await vm.ImportAsync(source);
        await first;

        await account.Received(1).ImportAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private YouTubeAccountViewModel CreateViewModel() =>
        new(_accountService, _coordinator, _appConfigStore);

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
