using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Watchbook.Configuration;

namespace Watchbook;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, IHttpClientFactory httpClientFactory)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _httpClientFactory = httpClientFactory;
    }

    IHttpClientFactory _httpClientFactory;

    public HttpClient GetHttpClient()
    {
        var httpClient = _httpClientFactory.CreateClient(NamedClient.Default);
        // httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Name, Version.ToString()));

        return httpClient;
    }


    static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public static void PrettyPrint(string name, object obj)
    {
        Console.Write("PRETTY: {0}", name);
        Console.WriteLine(JsonSerializer.Serialize(obj, _opts));
    }

    public override string Name => "Watchbook";

    public override Guid Id => Guid.Parse("9A4BA60A-3146-4A05-AEB1-7106FE9FFC3A");

#pragma warning disable CS8618
    public static Plugin Instance { get; private set; }
#pragma warning restore CS8618

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            }
        ];
    }
}
