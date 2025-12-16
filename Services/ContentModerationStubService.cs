namespace Azure_SearchProject.Services;

public class ContentModerationStubService : IContentModerationService
{
    public Task<ModerationResult> CheckAsync(Stream imageStream)
        => Task.FromResult(new ModerationResult(true, "Stub allowed"));
}
