using server.Enums;
using System.Text.Json.Serialization;
namespace server.Classes;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public Role Role { get; set; }
    public int CompanyId { get; set; }
    public string Company { get; set; }

    public User(int id, string username, Role role, int companyId, string company)
    {
        Id = id;
        Username = username;
        Role = role;
        CompanyId = companyId;
        Company = company;
    }
}