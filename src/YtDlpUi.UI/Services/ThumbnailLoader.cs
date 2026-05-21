using Avalonia.Media.Imaging;

namespace YtDlpUi.UI.Services;

public interface IThumbnailLoader
{
    Task<Bitmap?> LoadAsync(string url, CancellationToken cancellationToken = default);
}

public sealed class ThumbnailLoader : IThumbnailLoader
{
    private readonly HttpClient _httpClient;

    public ThumbnailLoader(HttpClient? httpClient = null) =>
        _httpClient = httpClient ?? new HttpClient();

    public async Task<Bitmap?> LoadAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return Bitmap.DecodeToWidth(stream, 240);
        }
        catch
        {
            return null;
        }
    }
}
