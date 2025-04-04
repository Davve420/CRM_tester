namespace server.Records;

public record UpdateSubjectRequest()
{
    public string OldName { get; set; }
    public string NewName { get; set; }
};