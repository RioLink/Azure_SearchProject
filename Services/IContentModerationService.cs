namespace Azure_SearchProject.Services;

public record ModerationResult(bool Allowed, string Reason);

public interface IContentModerationService
{
    Task<ModerationResult> CheckAsync(Stream imageStream);
}
