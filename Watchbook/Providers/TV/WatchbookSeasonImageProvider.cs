using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Watchbook.Providers.TV;

public class WatchbookSeasonImageProvider : IRemoteImageProvider, IHasOrder
{
    private readonly ILogger<WatchbookSeasonImageProvider> _log;
    public int Order => -2;
    public string Name => "Watchbook";

    public WatchbookSeasonImageProvider(ILogger<WatchbookSeasonImageProvider> logger)
    {
        _log = logger;
    }

    public bool Supports(BaseItem item) => item is Season;

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
            var res = await apiClient.GetMediaById(id, CancellationToken.None).ConfigureAwait(false);
            if (!res.Success)
            {
                _log.LogError("API Error: Code: {code} Type: {type} Message: {message}", res.Error?.Code, res.Error?.Type, res.Error?.Message);
                throw new Exception($"API Error: {res.Error?.Message}");
            }

            var media = res.Data!;

            if (media.CoverUrl != null)
            {
                list.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Primary,
                    Url = media.CoverUrl,
                });
            }

            if (media.BannerUrl != null)
            {
                list.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Backdrop,
                    Url = media.BannerUrl,
                });
            }

            if (media.LogoUrl != null)
            {
                list.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Logo,
                    Url = media.LogoUrl,
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
