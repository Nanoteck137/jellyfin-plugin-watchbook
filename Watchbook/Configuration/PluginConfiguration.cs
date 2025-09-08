using MediaBrowser.Model.Plugins;

namespace Watchbook.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        ApiBaseUrl = "https://watchbook.nanoteck137.net";
    }

    public string ApiBaseUrl { get; set; } = "https://myapi.example.com";
}
