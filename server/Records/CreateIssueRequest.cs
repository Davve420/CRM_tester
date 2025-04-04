namespace server.Records;

public record CreateIssueRequest()
{
    public string Email { get; set; }
    public string Title { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
};