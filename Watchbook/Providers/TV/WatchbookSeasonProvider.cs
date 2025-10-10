using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Watchbook.Utils;

namespace Watchbook.Providers.TV;

public class WatchbookSeasonProvider : IRemoteMetadataProvider<Season, SeasonInfo>, IHasOrder
{
    private readonly ILogger<WatchbookSeasonProvider> _log;
    public int Order => -2;
    public string Name => "Watchbook";

    public WatchbookSeasonProvider(ILogger<WatchbookSeasonProvider> logger)
    {
        _log = logger;
    }

    public async Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
    {
        var apiClient = Plugin.Instance.GetApiClient();

        var result = new MetadataResult<Season>();

        var dirName = Path.GetFileName(info.Path);

        info.SeriesProviderIds.TryGetValue(ProviderNames.Watchbook, out string? seriesId);
        if (string.IsNullOrWhiteSpace(seriesId))
        {
            return result;
        }

        var res = await apiClient.GetCollectionItems(seriesId, CancellationToken.None).ConfigureAwait(false);
        if (!res.Success)
        {
            _log.LogError("API Error: Code: {code} Type: {type} Message: {message}", res.Error?.Code, res.Error?.Type, res.Error?.Message);
            throw new Exception($"API Error: {res.Error?.Message}");
        }

        var col = res.Data!.Items.Find(i => i.SearchSlug == SlugHelper.Slugify(dirName));
        if (col != null)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(col.StartDate))
            {
                if (DateTime.TryParseExact(col.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                {
                    startDate = t;
                }
            }

            if (!string.IsNullOrEmpty(col.EndDate))
            {
                if (DateTime.TryParseExact(col.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                {
                    endDate = t;
                }
            }

            result.HasMetadata = true;
            result.Item = new Season
            {
                Name = col.CollectionName,
                IndexNumber = col.Position,
                Overview = col.Description,
                ProductionYear = startDate?.Year,
                PremiereDate = startDate,
                EndDate = endDate,
                CommunityRating = col.Score,
                Studios = [.. col.Creators],
                Tags = [.. col.Tags],
                ProviderIds = new Dictionary<string, string> { { ProviderNames.Watchbook, col.MediaId } },
            };
        }

        return result;
    }

    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<RemoteSearchResult>());
    }

    public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();
        return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
    }
}
