using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DesignBot.Services;

public class CloudflareImageService : IImageService
{
    private const string Model = "@cf/black-forest-labs/flux-1-schnell";

    private readonly HttpClient _http;
    private readonly string _accountId;
    private readonly string _apiToken;

    public CloudflareImageService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _accountId = (config["Cloudflare:AccountId"] ?? "").Trim();
        _apiToken = (config["Cloudflare:ApiToken"] ?? "").Trim();
    }

    public async Task<(string? DataUrl, string? Error)> GenerateImageAsync(
        string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_accountId) || string.IsNullOrWhiteSpace(_apiToken))
            return (null, "Keine Cloudflare-Zugangsdaten gefunden.");

        var fullPrompt = $"Modern, clean UI/web design mockup: {prompt}. "
                       + "High quality, professional, well-structured layout.";

        var url = $"https://api.cloudflare.com/client/v4/accounts/{_accountId}/ai/run/{Model}";

        try
        {
            var body = JsonSerializer.Serialize(new { prompt = fullPrompt, steps = 4 });

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return (null, $"Cloudflare-Fehler ({(int)response.StatusCode}): {ExtractError(json)}");

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("result", out var result) &&
                result.TryGetProperty("image", out var image))
            {
                var base64 = image.GetString();
                if (!string.IsNullOrEmpty(base64))
                    return ($"data:image/jpeg;base64,{base64}", null);
            }

            return (null, "Kein Bild in der Antwort erhalten. Versuch es nochmal.");
        }
        catch (Exception ex)
        {
            return (null, $"Verbindungsfehler: {ex.Message}");
        }
    }

    private static string ExtractError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0 &&
                errors[0].TryGetProperty("message", out var msg))
            {
                return msg.GetString() ?? json;
            }
        }
        catch
        {
        }

        return json.Length > 200 ? json[..200] : json;
    }
}
