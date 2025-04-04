namespace server.Records;

public record RegisterRequest()
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }
    public string Company { get; set; }
};