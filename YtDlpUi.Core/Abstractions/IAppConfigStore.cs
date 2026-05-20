using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IAppConfigStore
{
    string ConfigRoot { get; }
    Task<AppConfiguration> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppConfiguration configuration, CancellationToken cancellationToken = default);
    Task EnsureBootstrapAsync(CancellationToken cancellationToken = default);
}
