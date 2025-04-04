namespace server.Records;

public record UpdateUserRequest()
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
};