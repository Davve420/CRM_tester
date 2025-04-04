using server.Enums;
using System.Text.Json.Serialization;

namespace server.Classes;

public class Employee
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public Role Role { get; set; }

    public Employee(int id, string username, string firstname, string lastname, string email, Role role)
    {
        Id = id;
        Username = username;
        Firstname = firstname;
        Lastname = lastname;
        Email = email;
        Role = role;
    }
}