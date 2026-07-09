namespace DesignBot.Models;

public class ChatMessage
{
    public string Role { get; set; } = "user";

    public string Content { get; set; } = "";

    public string? ImageDataUrl { get; set; }

    public bool IsUser => Role == "user";
}
