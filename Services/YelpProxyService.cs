using System.Net.Http.Headers;

namespace PlayStudioServer.Services;

public class YelpProxyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly ILogger<YelpProxyService> _logger;

    public YelpProxyService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<YelpProxyService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _apiKey = configuration["YELP_API_KEY"]?.Trim().Replace("\n", "").Replace("\r", "").Replace(" ", "")
            ?? throw new ArgumentException("YELP_API_KEY is not configured");

        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Contains('\n') || _apiKey.Contains('\r'))
        {
            throw new ArgumentException("YELP_API_KEY contains invalid characters or is empty");
        }
    }

    public async Task<IResult> ProxyRequestAsync(HttpContext context)
    {
        var client = _httpClientFactory.CreateClient("yelp");
        client.Timeout = TimeSpan.FromSeconds(10);

        string path = context.Request.Path.Value?.Substring("/api/".Length) ?? string.Empty;
        string target = path + context.Request.QueryString.Value;

        var upstream = new HttpRequestMessage(new HttpMethod(context.Request.Method), target);

        if (context.Request.ContentLength.GetValueOrDefault() > 0 &&
            context.Request.Method is not "GET" and not "HEAD")
        {
            upstream.Content = new StreamContent(context.Request.Body);
            if (!string.IsNullOrWhiteSpace(context.Request.ContentType))
            {
                upstream.Content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType);
            }
        }

        CopyRequestHeaders(context, upstream);
        upstream.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using var response = await client.SendAsync(upstream, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

        context.Response.StatusCode = (int)response.StatusCode;
        CopyResponseHeaders(response, context);

        await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        return Results.Empty;
    }

    private static void CopyRequestHeaders(HttpContext context, HttpRequestMessage upstream)
    {
        foreach (var header in context.Request.Headers)
        {
            var name = header.Key;
            if (IsHopByHopHeader(name)) continue;

            if (!upstream.Headers.TryAddWithoutValidation(name, (IEnumerable<string>)header.Value))
                upstream.Content?.Headers.TryAddWithoutValidation(name, (IEnumerable<string>)header.Value);
        }
    }

    private static void CopyResponseHeaders(HttpResponseMessage response, HttpContext context)
    {
        foreach (var header in response.Headers)
            context.Response.Headers[header.Key] = header.Value.ToArray();
        foreach (var header in response.Content.Headers)
            context.Response.Headers[header.Key] = header.Value.ToArray();

        context.Response.Headers.Remove("transfer-encoding");
    }

    private static bool IsHopByHopHeader(string name) =>
        name.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Connection", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase);
}