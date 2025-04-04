namespace server.Records;

public record CreateEmployeeRequest()
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Password { get; set; }
};