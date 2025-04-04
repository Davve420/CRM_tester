namespace server.Records;

public record UpdateIssueStateRequest()
{
    public string NewState { get; init; }
};