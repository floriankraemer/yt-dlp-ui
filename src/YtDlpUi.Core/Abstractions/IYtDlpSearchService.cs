using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IYtDlpSearchService
{
    Task<SearchResultPage> SearchAsync(string query, int skip = 0, CancellationToken cancellationToken = default);
}
