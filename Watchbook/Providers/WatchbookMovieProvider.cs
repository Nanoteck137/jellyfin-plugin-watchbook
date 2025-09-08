using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Model.Entities;
using System.Globalization;

namespace Watchbook.Providers;

public class WatchbookMovieProvider : IRemoteMetadataProvider<Movie, MovieInfo>, IHasOrder
{
    private readonly ILogger<WatchbookMovieProvider> _log;
    public int Order => -2;
    public string Name => "Watchbook";

    public WatchbookMovieProvider(ILogger<WatchbookMovieProvider> logger)
    {
        _log = logger;
    }

    public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
    {
        Plugin.PrettyPrint("Movie GetMetadata", info);

        var id = info.GetProviderId(ProviderNames.Watchbook);
        if (id != null)
        {
            var client = new ApiClient("https://watchbook.nanoteck137.net");
            var res = await client.GetMediaById(id, CancellationToken.None).ConfigureAwait(false);
            Plugin.PrettyPrint("Initial API Call", res);

            if (!res.Success)
            {
                _log.LogError("API Error: Code: {code} Type: {type} Message: {message}", res.Error?.Code, res.Error?.Type, res.Error?.Message);
                throw new Exception($"API Error: {res.Error?.Message}");
            }

            var media = res.Data!;

            DateTime? startDate = null;

            if (!string.IsNullOrEmpty(media.StartDate))
            {
                if (DateTime.TryParseExact(media.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                {
                    startDate = t;
                }
            }

            var result = new MetadataResult<Movie>
            {
                HasMetadata = true,
                Item = new Movie
                {
                    Name = media.Title,
                    Overview = media.Description,
                    ProductionYear = startDate?.Year,
                    PremiereDate = startDate,
                    CriticRating = media.Score,
                    Studios = [.. media.Creators],
                    Tags = [.. media.Tags],
                    ProviderIds = new Dictionary<string, string> { { ProviderNames.Watchbook, media.Id } },
                }
            };

            return result;
        }
        else
        {
            var client = new ApiClient("https://watchbook.nanoteck137.net");
            var res = await client.GetMedia(string.Format("title % \"%{0}%\"", info.Name), CancellationToken.None).ConfigureAwait(false);
            Plugin.PrettyPrint("Initial API Call", res);

            var result = new MetadataResult<Movie>();
            if (res.Success && res.Data?.Media.Count > 0)
            {
                var media = res.Data.Media[0];

                DateTime? startDate = null;

                if (!string.IsNullOrEmpty(media.StartDate))
                {
                    if (DateTime.TryParseExact(media.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                    {
                        startDate = t;
                    }
                }

                result = new MetadataResult<Movie>
                {
                    HasMetadata = true,
                    Item = new Movie
                    {
                        Name = media.Title,
                        Overview = media.Description,
                        ProductionYear = startDate?.Year,
                        PremiereDate = startDate,
                        CriticRating = media.Score,
                        Studios = [.. media.Creators],
                        Tags = [.. media.Tags],
                        ProviderIds = new Dictionary<string, string> { { ProviderNames.Watchbook, media.Id } },
                    }
                };
            }

            return result;
        }
    }

    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
    {
        Plugin.PrettyPrint("Movie GetSearchResults", searchInfo);

        // searchInfo.ProviderIds
        searchInfo.ProviderIds.TryGetValue(ProviderNames.Watchbook, out var id);

        _log.LogInformation("GetSearchResults: Id: {id}", id);

        var client = new ApiClient("https://watchbook.nanoteck137.net");
        var res = await client.GetMedia(string.Format("title % \"%{0}%\"", searchInfo.Name), CancellationToken.None).ConfigureAwait(false);
        Plugin.PrettyPrint("Initial API Call", res);

        var results = new List<RemoteSearchResult>();

        foreach (var media in res.Data?.Media ?? [])
        {
            Plugin.PrettyPrint("Media Item", media);
            results.Add(
                new()
                {
                    Name = media.Title,
                    // ProductionYear = 2025,
                    SearchProviderName = ProviderNames.Watchbook,
                    ImageUrl = media.CoverUrl ?? "",
                    ProviderIds = new Dictionary<string, string> { { ProviderNames.Watchbook, media.Id } }
                }
            );
        }

        return results;
    }

    public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        _log.LogError("GetImageResponse: URL: {url}", url);

        var httpClient = Plugin.Instance.GetHttpClient();

        return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
    }
}
