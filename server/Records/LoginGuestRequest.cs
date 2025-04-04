namespace server.Records;

public record LoginGuestRequest()
{
    public string Email { get; set; }
    public Guid ChatId { get; set; }
};