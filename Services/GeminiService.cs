using System.Text;
using System.Text.Json;
using DesignBot.Models;

namespace DesignBot.Services;

public class GeminiService : IAiService
{
    private const string SystemPrompt = """
        Du bist "DesignBot", ein freundlicher und kompetenter KI-Design-Berater.
        Der Nutzer beschreibt dir eine Website-, App- oder Projekt-Idee.
        Deine Aufgabe: konkrete, sofort umsetzbare Designvorschläge geben.

        Gehe in deiner Antwort – wo sinnvoll – auf diese Punkte ein:
        - Stil/Stimmung (z. B. modern, verspielt, seriös, minimalistisch)
        - Farbpalette mit konkreten HEX-Codes (z. B. #2563EB)
        - Schriftarten-Vorschläge (am besten Google Fonts, je eine für Überschrift und Text)
        - Layout-Aufbau: welche Sektionen in welcher Reihenfolge
        - konkrete UI-Komponenten oder Elemente, die gut passen würden

        Antworte auf Deutsch, klar strukturiert mit Überschriften und Stichpunkten.
        Sei konkret statt allgemein. Halte dich kurz genug, dass es übersichtlich bleibt.
        """;

    private static readonly string[] Models =
    {
        "gemini-2.5-flash",
        "gemini-2.5-flash-lite",
        "gemini-2.5-pro",
        "gemini-3.5-flash",
        "gemini-3.1-pro-preview",
        "gemini-flash-latest",
    };

    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

    private readonly HttpClient _http;
    private readonly string _apiKey;

    public GeminiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = (config["GEMINI_API_KEY"] ?? config["Gemini:ApiKey"] ?? "").Trim();
    }

    public IReadOnlyList<string> AvailableModels => Models;

    public async Task<string> GetReplyAsync(
        IReadOnlyList<ChatMessage> history, string model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return "Kein API-Key gefunden (GEMINI_API_KEY bzw. Gemini:ApiKey setzen).";

        if (!Models.Contains(model))
            model = Models[0];

        var contents = history.Select(m => new
        {
            role = m.IsUser ? "user" : "model",
            parts = new[] { new { text = m.Content } }
        });

        var requestBody = new
        {
            system_instruction = new { parts = new[] { new { text = SystemPrompt } } },
            contents,
            generationConfig = new { temperature = 0.8 }
        };

        var url = $"{BaseUrl}{model}:generateContent?key={_apiKey}";

        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _http.PostAsync(url, content, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return $"Gemini-Fehler ({(int)response.StatusCode}): {ExtractError(responseText)}";

            return ExtractReply(responseText);
        }
        catch (Exception ex)
        {
            return $"Verbindungsfehler: {ex.Message}";
        }
    }

    private static string ExtractReply(string responseJson)
    {
        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var parts = candidates[0].GetProperty("content").GetProperty("parts");
            var sb = new StringBuilder();
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text))
                    sb.Append(text.GetString());
            }

            if (sb.Length > 0)
                return sb.ToString();
        }

        return "Keine Antwort erhalten. Versuch es nochmal oder wähle ein anderes Modell.";
    }

    private static string ExtractError(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            if (doc.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? responseJson;
            }
        }
        catch
        {
        }

        return responseJson;
    }
}
