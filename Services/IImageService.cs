namespace DesignBot.Services;

public interface IImageService
{
    Task<(string? DataUrl, string? Error)> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default);
}
