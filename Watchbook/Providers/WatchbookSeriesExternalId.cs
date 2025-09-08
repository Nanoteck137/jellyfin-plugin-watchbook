using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Watchbook.Providers;

public class WatchbookSeriesExternalId : IExternalId
{
    public bool Supports(IHasProviderIds item) => item is Series;

    public string ProviderName => "Watchbook";

    public string Key => ProviderNames.Watchbook;

    public ExternalIdMediaType? Type => ExternalIdMediaType.Series;

    public string UrlFormatString => "https://watchbook.nanoteck137.net/collections/{0}";
}
