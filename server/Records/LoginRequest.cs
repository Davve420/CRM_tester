namespace server.Records;

public record LoginRequest()
{
    public string Email { get; set; }
    public string Password { get; set; }
};