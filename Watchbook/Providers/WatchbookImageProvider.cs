using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using Watchbook.Api;

namespace Watchbook.Providers;

public class WatchbookImageProvider : IRemoteImageProvider, IHasOrder
{
    private readonly ILogger<WatchbookImageProvider> _log;
    public int Order => -2;
    public string Name => "Watchbook";

    public WatchbookImageProvider(ILogger<WatchbookImageProvider> logger)
    {
        _log = logger;
    }

    public bool Supports(BaseItem item) => item is Series || item is Season || item is Movie;

    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return [ImageType.Primary, ImageType.Backdrop, ImageType.Logo];
    }

    public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
    {
        var apiClient = Plugin.Instance.GetApiClient();

        if (item is Movie)
        {
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

        if (item is Series)
        {
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

        if (item is Season)
        {
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

        return [];
    }

    public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();
        return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
    }
}
