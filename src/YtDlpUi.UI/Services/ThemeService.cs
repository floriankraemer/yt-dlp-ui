using Avalonia;
using Avalonia.Styling;
using YtDlpUi.Core.Models;

namespace YtDlpUi.UI.Services;

public static class ThemeService
{
    public static ThemeVariant MapToVariant(ThemePreference preference) => preference switch
    {
        ThemePreference.Light => ThemeVariant.Light,
        ThemePreference.Dark => ThemeVariant.Dark,
        _ => ThemeVariant.Default,
    };

    public static void Apply(ThemePreference preference)
    {
        if (Application.Current is null)
            return;

        Application.Current.RequestedThemeVariant = MapToVariant(preference);
    }
}
