// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.UI.ViewModels;

public sealed class YouTubeAccountViewModel : ViewModelBase
{
    public const string CookiesExtensionHelpUrl =
        "https://chromewebstore.google.com/detail/get-cookiestxt-locally/cclelndahbckbenkjhflpdbgdldlbecc";

    private readonly IYouTubeAccountService _youTubeAccountService;
    private readonly SettingsCoordinator _settingsCoordinator;
    private readonly IAppConfigStore _appConfigStore;

    private string _status = "Not signed in";
    private string _cookiesPathDisplay = string.Empty;
    private string? _warningMessage;
    private string? _testResult;
    private string? _errorMessage;
    private bool _isBusy;
    private bool _isSignedIn;

    public YouTubeAccountViewModel(
        IYouTubeAccountService youTubeAccountService,
        SettingsCoordinator settingsCoordinator,
        IAppConfigStore appConfigStore)
    {
        _youTubeAccountService = youTubeAccountService;
        _settingsCoordinator = settingsCoordinator;
        _appConfigStore = appConfigStore;
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public bool IsSignedIn
    {
        get => _isSignedIn;
        private set => SetProperty(ref _isSignedIn, value);
    }

    public string CookiesPathDisplay
    {
        get => _cookiesPathDisplay;
        private set => SetProperty(ref _cookiesPathDisplay, value);
    }

    public string? WarningMessage
    {
        get => _warningMessage;
        private set => SetProperty(ref _warningMessage, value);
    }

    public string? TestResult
    {
        get => _testResult;
        private set => SetProperty(ref _testResult, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public bool IsNotSignedIn => !IsSignedIn;

    public void Refresh()
    {
        var path = _youTubeAccountService.ResolveCookiesPath();
        IsSignedIn = path is not null;
        Status = IsSignedIn ? "Signed in" : "Not signed in";
        CookiesPathDisplay = path ?? string.Empty;
        OnPropertyChanged(nameof(IsNotSignedIn));
    }

    public async Task ImportAsync(string sourcePath)
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = null;
        WarningMessage = null;
        TestResult = null;

        try
        {
            var result = await _youTubeAccountService.ImportAsync(sourcePath);
            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "Failed to import cookies.";
                return;
            }

            WarningMessage = result.Warning;
            Refresh();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SignOutAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = null;
        WarningMessage = null;
        TestResult = null;

        try
        {
            await _youTubeAccountService.SignOutAsync();
            Refresh();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task TestAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = null;
        TestResult = null;

        try
        {
            var config = await _appConfigStore.LoadAsync();
            var result = await _settingsCoordinator.TestYouTubeCookiesAsync(config);
            if (string.IsNullOrWhiteSpace(result))
            {
                TestResult = "Cookies test failed.";
                return;
            }

            if (result.Contains("not found", StringComparison.OrdinalIgnoreCase)
                || result.Contains("failed", StringComparison.OrdinalIgnoreCase)
                || result.Contains("Not signed in", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = result;
                return;
            }

            TestResult = result;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
