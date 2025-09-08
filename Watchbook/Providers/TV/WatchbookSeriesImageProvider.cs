using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Watchbook.Providers.TV;

public class WatchbookSeriesImageProvider : IRemoteImageProvider, IHasOrder
{
    private readonly ILogger<WatchbookSeriesImageProvider> _log;
    public int Order => -2;
    public string Name => "Watchbook";

    public WatchbookSeriesImageProvider(ILogger<WatchbookSeriesImageProvider> logger)
    {
        _log = logger;
    }

    public bool Supports(BaseItem item) => item is Series;

    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return [ImageType.Primary, ImageType.Backdrop, ImageType.Logo];
    }

    public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
    {
        var apiClient = Plugin.Instance.GetApiClient();

        var id = item.GetProviderId(ProviderNames.Watchbook);
        var list = new List<RemoteImageInfo>();

        if (!string.IsNullOrEmpty(id))
        {
            var res = await apiClient.GetCollectionById(id, CancellationToken.None).ConfigureAwait(false);
            if (!res.Success)
            {
                _log.LogError("API Error: Code: {code} Type: {type} Message: {message}", res.Error?.Code, res.Error?.Type, res.Error?.Message);
                throw new Exception($"API Error: {res.Error?.Message}");
            }

            var col = res.Data!;

            if (col.CoverUrl != null)
            {
                list.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Primary,
                    Url = col.CoverUrl,
                });
            }

            if (col.BannerUrl != null)
            {
                list.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Backdrop,
                    Url = col.BannerUrl,
                });
            }

            if (col.LogoUrl != null)
            {
                list.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Logo,
                    Url = col.LogoUrl,
                });
            }
        }

        return list;
    }

    public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();
        return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
    }
}
