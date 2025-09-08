using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System.Globalization;
using MediaBrowser.Model.Entities;
using Jellyfin.Data.Entities;
using Watchbook.Utils;
using ICU4N.Util;

namespace Watchbook.Providers;


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
        Plugin.PrettyPrint("Season GetMetadata: SeasonInfo", info);

        var dirName = Path.GetFileName(info.Path);

        var result = new MetadataResult<Season>();

        info.SeriesProviderIds.TryGetValue(ProviderNames.Watchbook, out string? seriesId);

        if (string.IsNullOrWhiteSpace(seriesId))
        {
            return result;
        }

        var client = new ApiClient("https://watchbook.nanoteck137.net");
        var res = await client.GetCollectionItems(seriesId, CancellationToken.None).ConfigureAwait(false);
        Plugin.PrettyPrint("Season GetMetadata: ", res);

        if (!res.Success)
        {
            _log.LogError("API Error: Code: {code} Type: {type} Message: {message}", res.Error?.Code, res.Error?.Type, res.Error?.Message);
            throw new Exception($"API Error: {res.Error?.Message}");
        }

        var items = res.Data!.Items;

        _log.LogInformation("Slug: {slug}", SlugHelper.Slugify(dirName));

        var col = items.Find(i => i.SearchSlug == SlugHelper.Slugify(dirName));
        Plugin.PrettyPrint("Season Col: ", col);
        if (col != null)
        {
            _log.LogInformation("Found matching season {season} for series {series}", info.Name, seriesId);
            result.HasMetadata = true;
            result.Item = new Season
            {
                Name = col.CollectionName,
                // TODO(patrik): Temp
                IndexNumber = col.Order + 1,
                Overview = col.Description,
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
        _log.LogError("GetImageResponse: URL: {url}", url);

        var httpClient = Plugin.Instance.GetHttpClient();

        return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
    }
}
