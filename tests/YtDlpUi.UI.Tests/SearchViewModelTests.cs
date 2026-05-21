using NSubstitute;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;
using YtDlpUi.UI.Services;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class SearchViewModelTests
{
    [Fact]
    public async Task SearchAsync_PopulatesResults()
    {
        var searchService = Substitute.For<IYtDlpSearchService>();
        searchService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new SearchResultPage
            {
                Query = "cats",
                Results =
                [
                    new YouTubeSearchResult
                    {
                        VideoId = "v1",
                        Title = "Cat Video",
                        Channel = "Cat Channel",
                        WatchUrl = "https://www.youtube.com/watch?v=v1",
                    },
                ],
            });

        var appConfig = Substitute.For<IAppConfigStore>();
        appConfig.LoadAsync(Arg.Any<CancellationToken>())
            .Returns(new AppConfiguration { ActiveProfileId = "default" });

        var profileStore = Substitute.For<IProfileStore>();
        profileStore.ListAsync(Arg.Any<CancellationToken>())
            .Returns([new DownloadProfile { Id = "default", Name = "Default" }]);

        var vm = new SearchViewModel(
            searchService,
            appConfig,
            profileStore,
            CreateEnqueueCoordinator(),
            Substitute.For<IThumbnailLoader>());

        await vm.InitializeAsync();
        vm.SearchQuery = "cats";
        await vm.SearchAsync();

        Assert.True(vm.HasResults);
        Assert.Single(vm.Results);
        Assert.Equal("Cat Video", vm.Results[0].Title);
        Assert.Equal("1 video found.", vm.StatusMessage);
    }

    [Fact]
    public async Task SearchAsync_ServiceError_SetsErrorMessage()
    {
        var searchService = Substitute.For<IYtDlpSearchService>();
        searchService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<SearchResultPage>>(_ => throw new InvalidOperationException("Search failed"));

        var vm = new SearchViewModel(
            searchService,
            Substitute.For<IAppConfigStore>(),
            Substitute.For<IProfileStore>(),
            CreateEnqueueCoordinator(),
            Substitute.For<IThumbnailLoader>());

        vm.SearchQuery = "test";
        await vm.SearchAsync();

        Assert.Equal("Search failed", vm.ErrorMessage);
        Assert.False(vm.HasResults);
    }

    private static DownloadEnqueueCoordinator CreateEnqueueCoordinator() =>
        new(
            Substitute.For<IDownloadQueueService>(),
            Substitute.For<IAppConfigStore>(),
            Substitute.For<IProfileStore>(),
            new YouTubeUrlNormalizer(),
            new BinaryLocator(Path.GetTempPath()),
            new DownloadFolderService());
}
