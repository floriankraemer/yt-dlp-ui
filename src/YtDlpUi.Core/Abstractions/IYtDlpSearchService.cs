using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IYtDlpSearchService
{
    Task<SearchResultPage> SearchAsync(string query, CancellationToken cancellationToken = default);
}
