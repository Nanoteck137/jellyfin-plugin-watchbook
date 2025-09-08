using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Watchbook.Providers;

public class ApiError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public class ApiResult<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    public ApiError? Error { get; set; }
}

public class Page
{
    [JsonPropertyName("page")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("perPage")]
    public int PerPage { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

public class Media
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    // TmdbId    string `json:"tmdbId"`
    // ImdbId    string `json:"imdbId"`
    // MalId     string `json:"malId"`
    // AnilistId string `json:"anilistId"`

    [JsonPropertyName("mediaType")]
    public string MediaType { get; set; }

    [JsonPropertyName("score")]
    public float? Score { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("rating")]
    public string Rating { get; set; }

    [JsonPropertyName("partCount")]
    public int PartCount { get; set; }

    [JsonPropertyName("airingSeason")]
    public string? AiringSeason { get; set; }

    [JsonPropertyName("startDate")]
    public string? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public string? EndDate { get; set; }

    [JsonPropertyName("creators")]
    public List<string> Creators { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    [JsonPropertyName("coverUrl")]
    public string? CoverUrl { get; set; }

    [JsonPropertyName("bannerUrl")]
    public string? BannerUrl { get; set; }

    [JsonPropertyName("logoUrl")]
    public string? LogoUrl { get; set; }

    // User    *MediaUser    `json:"user,omitempty"`
    // Release *MediaRelease `json:"release"`
}

public class GetMedia
{
    [JsonPropertyName("page")]
    public required Page Page { get; set; }

    [JsonPropertyName("media")]
    public required List<Media> Media { get; set; }
}

public class Collection
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("collectionType")]
    public string CollectionType { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("coverUrl")]
    public string? CoverUrl { get; set; }

    [JsonPropertyName("bannerUrl")]
    public string? BannerUrl { get; set; }

    [JsonPropertyName("logoUrl")]
    public string? LogoUrl { get; set; }
}

public class GetCollections
{
    [JsonPropertyName("page")]
    public required Page Page { get; set; }

    [JsonPropertyName("collections")]
    public required List<Collection> Collections { get; set; }
}

public class CollectionItem
{
    [JsonPropertyName("collectionId")]
    public string CollectionId { get; set; }

    [JsonPropertyName("mediaId")]
    public string MediaId { get; set; }

    [JsonPropertyName("collectionName")]
    public string CollectionName { get; set; }

    [JsonPropertyName("searchSlug")]
    public string SearchSlug { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("mediaType")]
    public string MediaType { get; set; }

    [JsonPropertyName("score")]
    public float? Score { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("rating")]
    public string Rating { get; set; }

    [JsonPropertyName("partCount")]
    public int PartCount { get; set; }

    [JsonPropertyName("airingSeason")]
    public string? AiringSeason { get; set; }

    [JsonPropertyName("startDate")]
    public string? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public string? EndDate { get; set; }

    [JsonPropertyName("creators")]
    public List<string> Creators { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    [JsonPropertyName("coverUrl")]
    public string? CoverUrl { get; set; }

    [JsonPropertyName("bannerUrl")]
    public string? BannerUrl { get; set; }

    [JsonPropertyName("logoUrl")]
    public string? LogoUrl { get; set; }
}

public class GetCollectionItems
{
    [JsonPropertyName("items")]
    public List<CollectionItem> Items { get; set; }
}

public class ApiClient
{
    public ApiClient(string baseUrl)
    {
        BaseUrl = baseUrl.TrimEnd('/');
    }

    public string BaseUrl { get; }

    public async Task<ApiResult<GetMedia>> GetMedia(string filter, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();

        var url = BaseUrl + "/api/v1/media?filter=" + Uri.EscapeDataString(filter);

        // using HttpContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        using var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<ApiResult<GetMedia>>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false) ?? throw new Exception("Failed to deserialize API response");
    }

    public async Task<ApiResult<Media>> GetMediaById(string id, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();

        var url = BaseUrl + string.Format("/api/v1/media/{0}", id);

        // using HttpContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        using var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<ApiResult<Media>>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false) ?? throw new Exception("Failed to deserialize API response");
    }

    public async Task<ApiResult<GetCollections>> GetCollections(string filter, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();

        var url = BaseUrl + "/api/v1/collections?filter=" + Uri.EscapeDataString(filter);

        // using HttpContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        using var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<ApiResult<GetCollections>>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false) ?? throw new Exception("Failed to deserialize API response");
    }

    public async Task<ApiResult<Collection>> GetCollectionById(string id, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();

        var url = BaseUrl + string.Format("/api/v1/collections/{0}", id);

        // using HttpContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        using var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<ApiResult<Collection>>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false) ?? throw new Exception("Failed to deserialize API response");
    }

    public async Task<ApiResult<GetCollectionItems>> GetCollectionItems(string id, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();

        var url = BaseUrl + string.Format("/api/v1/collections/{0}/items", id);

        // using HttpContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        using var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<ApiResult<GetCollectionItems>>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false) ?? throw new Exception("Failed to deserialize API response");
    }
}

public class WatchbookImageProvider : IRemoteImageProvider
{
    private readonly ILogger<WatchbookImageProvider> _log;

    public WatchbookImageProvider(ILogger<WatchbookImageProvider> logger)
    {
        _log = logger;
    }

    public string Name => "Watchbook";

    public bool Supports(BaseItem item) => item is Series || item is Season || item is Movie;

    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return [ImageType.Primary, ImageType.Backdrop, ImageType.Logo];
    }

    public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
    {
        if (item is Movie)
        {
            var id = item.GetProviderId(ProviderNames.Watchbook);
            var list = new List<RemoteImageInfo>();

            if (!string.IsNullOrEmpty(id))
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
                var client = new ApiClient("https://watchbook.nanoteck137.net");
                var res = await client.GetCollectionById(id, CancellationToken.None).ConfigureAwait(false);
                Plugin.PrettyPrint("Initial API Call", res);

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
                var client = new ApiClient("https://watchbook.nanoteck137.net");
                var res = await client.GetMediaById(id, CancellationToken.None).ConfigureAwait(false);
                Plugin.PrettyPrint("Initial API Call", res);

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

        Plugin.PrettyPrint("GetImages", item);
        var seriesId = item.GetProviderId(ProviderNames.Watchbook);
        return await GetImages(seriesId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<RemoteImageInfo>> GetImages(string? aid, CancellationToken cancellationToken)
    {
        var list = new List<RemoteImageInfo>();

        if (!string.IsNullOrEmpty(aid))
        {
            list.Add(new RemoteImageInfo
            {
                ProviderName = Name,
                Type = ImageType.Primary,
                Url = "https://cdn.myanimelist.net/images/anime/1744/150433.jpg",
            });
        }

        return list;
    }

    public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        var httpClient = Plugin.Instance.GetHttpClient();
        return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
    }
}
