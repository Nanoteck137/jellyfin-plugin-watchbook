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
using Watchbook.Api;

namespace Watchbook.Providers;

public class WatchbookSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
{
    private readonly ILogger<WatchbookSeriesProvider> _log;
    public int Order => -2;
    public string Name => "Watchbook";

    public WatchbookSeriesProvider(ILogger<WatchbookSeriesProvider> logger)
    {
        _log = logger;
    }

    public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
    {
        Plugin.PrettyPrint("Series GetMetadata", info);

        var apiClient = Plugin.Instance.GetApiClient();

        var id = info.GetProviderId(ProviderNames.Watchbook);
        if (id != null)
        {
            var res = await apiClient.GetCollectionById(id, CancellationToken.None).ConfigureAwait(false);
            Plugin.PrettyPrint("GetMetadata: GetCollectionById API Call", res);

            if (!res.Success)
            {
                _log.LogError("API Error: Code: {code} Type: {type} Message: {message}", res.Error?.Code, res.Error?.Type, res.Error?.Message);
                throw new Exception($"API Error: {res.Error?.Message}");
            }

            var col = res.Data!;

            var result = new MetadataResult<Series>
            {
                HasMetadata = true,
                Item = new Series
                {
                    Name = col.Name,
                    ProviderIds = new Dictionary<string, string> { { ProviderNames.Watchbook, col.Id } },
                }
            };

            return result;
        }
        else
        {
            var res = await apiClient.GetCollections(string.Format("name % \"%{0}%\"", info.Name), CancellationToken.None).ConfigureAwait(false);
            Plugin.PrettyPrint("Initial API Call", res);

            var result = new MetadataResult<Series>();
            if (res.Success && res.Data?.Collections.Count > 0)
            {
                var col = res.Data.Collections[0];

                result = new MetadataResult<Series>
                {
                    HasMetadata = true,
                    Item = new Series
                    {
                        Name = col.Name,
                        ProviderIds = new Dictionary<string, string> { { ProviderNames.Watchbook, col.Id } },
                    }
                };
            }

            return result;
        }
    }

    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
    {
        var apiClient = Plugin.Instance.GetApiClient();

        // searchInfo.ProviderIds
        // searchInfo.ProviderIds.TryGetValue(ProviderNames.Watchbook, out var id);

        var res = await apiClient.GetCollections(string.Format("name % \"%{0}%\"", searchInfo.Name), CancellationToken.None).ConfigureAwait(false);
        // TODO(patrik): Check res

        var results = new List<RemoteSearchResult>();

        foreach (var col in res.Data?.Collections ?? [])
        {
            results.Add(
                new()
                {
                    Name = col.Name,
                    SearchProviderName = ProviderNames.Watchbook,
                    ImageUrl = col.CoverUrl ?? "",
                    ProviderIds = new Dictionary<string, string> { { ProviderNames.Watchbook, col.Id } }
                }
            );
        }

        return results;
    }

    public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();
        return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
    }
}
