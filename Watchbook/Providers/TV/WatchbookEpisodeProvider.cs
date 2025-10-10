using System.Globalization;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Watchbook.Providers.TV;

public class WatchbookEpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>, IHasOrder
{
    private readonly ILogger<WatchbookEpisodeProvider> _log;
    public int Order => -2;
    public string Name => "Watchbook";

    public WatchbookEpisodeProvider(ILogger<WatchbookEpisodeProvider> logger)
    {
        _log = logger;
    }

    public async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
    {
        var apiClient = Plugin.Instance.GetApiClient();

        var result = new MetadataResult<Episode>();

        info.SeasonProviderIds.TryGetValue(ProviderNames.Watchbook, out string? mediaId);

        if (mediaId != null)
        {
            var res = await apiClient.GetMediaParts(mediaId, CancellationToken.None).ConfigureAwait(false);
            if (!res.Success)
            {
                _log.LogError("API Error: Code: {code} Type: {type} Message: {message}", res.Error?.Code, res.Error?.Type, res.Error?.Message);
                throw new Exception($"API Error: {res.Error?.Message}");
            }

            var episode = res.Data!.Parts.Find(p => p.Index == info.IndexNumber);
            if (episode != null)
            {
                DateTime? releaseDate = null;

                if (!string.IsNullOrEmpty(episode.ReleaseDate))
                {
                    if (DateTime.TryParseExact(episode.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                    {
                        releaseDate = t;
                    }
                }

                result.HasMetadata = true;
                result.Item = new Episode
                {
                    Name = episode.Name,
                    IndexNumber = info.IndexNumber,
                    PremiereDate = releaseDate,
                    ProviderIds = new Dictionary<string, string> { { ProviderNames.Watchbook, episode.MediaId + "/" + episode.Index } },
                };
            }
        }
        /*
        else
        {
            var client = new ApiClient("https://watchbook.nanoteck137.net");
            var res = await client.GetCollections(string.Format("name % \"%{0}%\"", info.Name), CancellationToken.None).ConfigureAwait(false);
            Plugin.PrettyPrint("Initial API Call", res);

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
        }*/

        return result;
    }

    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo, CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<RemoteSearchResult>>([]);
    }

    public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();
        return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
    }
}
