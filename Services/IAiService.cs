using DesignBot.Models;

namespace DesignBot.Services;

public interface IAiService
{
    IReadOnlyList<string> AvailableModels { get; }

    Task<string> GetReplyAsync(IReadOnlyList<ChatMessage> history, string model, CancellationToken cancellationToken = default);
}
