# DesignBot

Ein KI-Design-Berater als Web-Chatbot. Beschreibe eine Website- oder App-Idee, und
DesignBot schlägt dir **Farben (HEX), Schriften, Layout und Stil** vor.

Gebaut mit **Blazor** & **C# (.NET 8)**, die KI läuft über die **Google-Gemini-API**.

## Features

- Echter Chat mit Nachrichten-Verlauf
- Mehrere Gemini-Modelle live im Dropdown wählbar
- **Bild-Modus**: generiert Design-Mockups als Bild (Cloudflare Workers AI / FLUX.1-schnell)
- Konkrete, umsetzbare Designvorschläge
- Austauschbare AI-Dienste (`IAiService`, `IImageService`) – Anbieter leicht ersetzbar

## Lokal starten

1. **.NET 8 SDK** installiert haben ([Download](https://dotnet.microsoft.com/download)).
2. Einen **Gemini-API-Key** holen (kostenlos): https://aistudio.google.com/apikey
3. Key setzen (eine der beiden Varianten):

   **Variante A – Umgebungsvariable:**
   ```bash
   export GEMINI_API_KEY="DEIN_KEY"
   dotnet run
   ```

   **Variante B – User-Secrets (Key bleibt aus dem Code raus):**
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Gemini:ApiKey" "DEIN_KEY"
   dotnet run
   ```

4. *(Optional, für den Bild-Modus)* Kostenlosen **Cloudflare Workers AI**-Zugang anlegen
   (keine Kreditkarte) und hinterlegen:
   ```bash
   dotnet user-secrets set "Cloudflare:AccountId" "DEINE_ACCOUNT_ID"
   dotnet user-secrets set "Cloudflare:ApiToken"  "DEIN_API_TOKEN"
   ```
   Ohne diese Werte funktioniert die Text-Beratung trotzdem; nur die Bild-Generierung ist deaktiviert.

5. Im Browser öffnen: die in der Konsole angezeigte Adresse (z. B. `http://localhost:5000`).

## Projektstruktur

| Datei | Zweck |
|-------|-------|
| `Components/Pages/Chat.razor` | Die Chat-Oberfläche |
| `Services/GeminiService.cs` | Ruft die Gemini-API auf, enthält den „Design-Berater"-Prompt |
| `Services/IAiService.cs` | Schnittstelle – macht den AI-Anbieter austauschbar |
| `Models/ChatMessage.cs` | Modell einer einzelnen Nachricht |

## Online stellen

Die App ist eine Blazor-Server-App. Sie lässt sich z. B. kostenlos auf
[Render.com](https://render.com) hosten. Dort als **Environment-Variablen** hinterlegen
(nicht in den Code/Repo!):

| Variable | Zweck |
|----------|-------|
| `Gemini__ApiKey` | Text-Beratung (Google Gemini) |
| `Cloudflare__AccountId` | Bild-Modus (Cloudflare Workers AI) |
| `Cloudflare__ApiToken` | Bild-Modus (Cloudflare Workers AI) |

> Hinweis: Doppelter Unterstrich `__` = Doppelpunkt in der .NET-Konfiguration
> (`Gemini__ApiKey` entspricht `Gemini:ApiKey`).

---

> Hinweis: Der API-Key wird **nie** im Repository gespeichert (siehe `.gitignore`).
