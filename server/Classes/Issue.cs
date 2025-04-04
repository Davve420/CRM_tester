using server.Enums;
using System.Text.Json.Serialization;
namespace server.Classes;

public class Issue
{
    public Guid Id { get; set; }
    public String CompanyName { get; set; }
    public String CustomerEmail { get; set; }
    public String Subject { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public IssueState State { get; set; }
    public String Title { get; set; }
    public DateTime Created { get; set; }
    public DateTime Latest { get; set; }

    public Issue(Guid id, String companyName, string customerEmail, string subject, IssueState state, string title, DateTime created, DateTime latest)
    {
        Id = id;
        CompanyName = companyName;
        CustomerEmail = customerEmail;
        Subject = subject;
        State = state;
        Title = title;
        Created = created;
        Latest = latest;
    }
}