using System.Text.Json;
using Npgsql;
using server.Authorization;
using server.Classes;
using server.Enums;
using server.Records;
using server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using server.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace server.api;

[ApiController]
[Route("api/[controller]")]
public class IssuesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private NpgsqlDataSource Db;

    public IssuesController(ApplicationDbContext context, IConfiguration configuration, NpgsqlDataSource db)
    {
        _context = context;
        _configuration = configuration;
        Db = db;
    }

    [HttpPost]
    public async Task<ActionResult<Issue>> CreateIssue([FromBody] IssueRequest request)
    {
        if (request == null)
        {
            return BadRequest("Ogiltig förfrågan");
        }

        var issue = new Issue(
            id: Guid.NewGuid(),
            companyName: "Default Company", // Detta kan ändras senare
            customerEmail: request.Email,
            subject: request.Subject,
            state: IssueState.NEW,
            title: request.Title,
            created: DateTime.UtcNow,
            latest: DateTime.UtcNow
        );

        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();

        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            if (smtpSettings == null)
            {
                return CreatedAtAction(nameof(GetIssue), new { id = issue.Id }, issue);
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Support", smtpSettings["FromAddress"]));
            message.To.Add(new MailboxAddress("", issue.CustomerEmail));
            message.Subject = $"Nytt ärende: {issue.Title}";
            message.Body = new TextPart("plain")
            {
                Text = $"Ett nytt ärende har skapats:\n\nÄmne: {issue.Subject}\nMeddelande: {request.Message}"
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpSettings["Host"], int.Parse(smtpSettings["Port"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpSettings["Username"], smtpSettings["Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        catch (Exception ex)
        {
            // Logga felet men låt ärendet skapas ändå
            Console.WriteLine($"Kunde inte skicka e-post: {ex.Message}");
        }

        return CreatedAtAction(nameof(GetIssue), new { id = issue.Id }, issue);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Issue>> GetIssue(Guid id)
    {
        var issue = await _context.Issues.FindAsync(id);
        if (issue == null)
        {
            return NotFound();
        }
        return issue;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Issue>>> GetIssues()
    {
        return await _context.Issues.ToListAsync();
    }

    private async Task<IResult> GetIssueByCompany(HttpContext context)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));

        await using var cmd = Db.CreateCommand("SELECT * FROM companies_issues WHERE company_name = @company");
        cmd.Parameters.AddWithValue("@company", user.Company);

        try
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
               List<Issue> issuesList = new List<Issue>();
               while (reader.Read())
               {
                   issuesList.Add(new Issue(
                       reader.GetGuid(reader.GetOrdinal("id")),
                       reader.GetString(reader.GetOrdinal("company_name")),
                       reader.GetString(reader.GetOrdinal("customer_email")),
                       reader.GetString(reader.GetOrdinal("subject")),
                       Enum.Parse<IssueState>(reader.GetString(reader.GetOrdinal("state"))),
                       reader.GetString(reader.GetOrdinal("title")),
                       reader.GetDateTime(reader.GetOrdinal("created")),
                       reader.GetDateTime(reader.GetOrdinal("latest"))
                       ));
               }

               if (issuesList.Count > 0)
               {
                   return Results.Ok(new { issues = issuesList });
               }
               else
               {
                   return Results.NotFound(new { message = "No issues found." });
               }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }
    }
    
    private async Task<IResult> GetIssue(Guid issueId, HttpContext context)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));
        
        await using var cmd = Db.CreateCommand("SELECT * FROM companies_issues WHERE id = @issue_id AND company_name = @company_name");
        cmd.Parameters.AddWithValue("@issue_id", issueId);
        cmd.Parameters.AddWithValue("@company_name", user.Company);

        try
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                Issue issue = null;
                while (await reader.ReadAsync())
                {
                    issue = new Issue(
                        reader.GetGuid(reader.GetOrdinal("id")),
                        reader.GetString(reader.GetOrdinal("company_name")),
                        reader.GetString(reader.GetOrdinal("customer_email")),
                        reader.GetString(reader.GetOrdinal("subject")),
                        Enum.Parse<IssueState>(reader.GetString(reader.GetOrdinal("state"))),
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetDateTime(reader.GetOrdinal("created")),
                        reader.GetDateTime(reader.GetOrdinal("latest"))
                    );
                }

                if (issue != null)
                {
                    return Results.Ok(issue);
                }
                else
                {
                    return Results.NotFound(new { message = "No issue found." });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }
    }
    
    private async Task<IResult> UpdateIssueState(Guid issueId, HttpContext context, UpdateIssueStateRequest updateIssueStateRequest)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));
        
        await using var cmd = Db.CreateCommand("UPDATE issues SET state = @state::issue_state WHERE id = @issue_id AND company_id = @company_id");
        cmd.Parameters.AddWithValue("@state", Enum.Parse<IssueState>(updateIssueStateRequest.NewState).ToString());
        cmd.Parameters.AddWithValue("@issue_id", issueId);
        cmd.Parameters.AddWithValue("@company_id", user.CompanyId);

        try
        {
            var reader = await cmd.ExecuteNonQueryAsync();
            if (reader == 1)
            {
                return Results.Ok(new { message = "Issue state was updated." });
            }
            else
            {
                return Results.Conflict(new { message = "Query executed but something went wrong." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }    
    }

    private async Task<IResult> GetMessages(Guid issueId, HttpContext context)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));

        await using var cmd = user.Role == Role.GUEST
            ? Db.CreateCommand("SELECT * FROM issues WHERE id = @issue_id AND customer_email = @customer_email")
            : Db.CreateCommand("SELECT * FROM issues WHERE id = @issue_id AND company_id = @company_id");

        cmd.Parameters.AddWithValue("@issue_id", issueId);
        if (user.Role == Role.GUEST)
        {
            cmd.Parameters.AddWithValue("@customer_email", user.Username);
        }   
        else {
            cmd.Parameters.AddWithValue("@company_id", user.CompanyId);
        } 
        
        var reader = await cmd.ExecuteScalarAsync();
        if (reader == null)
        {
            return Results.Conflict(new { message = "You dont have access to messages." });
        }

        await using var cmd2 = Db.CreateCommand("SELECT * FROM issue_messages WHERE issue_id = @issue_id");
        cmd2.Parameters.AddWithValue("@issue_id", issueId);

        try
        {
            await using (var reader2 = await cmd2.ExecuteReaderAsync())
            {
                List<Message> messageList = new List<Message>();
                while (reader2.Read())
                {
                    messageList.Add(new Message(
                        reader2.GetString(reader2.GetOrdinal("message")),
                        reader2.GetString(reader2.GetOrdinal("sender")),
                        reader2.GetString(reader2.GetOrdinal("username")),
                        reader2.GetDateTime(reader2.GetOrdinal("time"))
                        ));
                }

                if (messageList.Count > 0)
                {
                    return Results.Ok(new { messages = messageList});
                }
                else
                {
                    return Results.NotFound(new { message = "No messages found." });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }
    }
    
    private async Task<IResult> CreateMessage(Guid issueId, HttpContext context, CreateMessageRequest createMessageRequest)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));

        await using var cmd = user.Role == Role.GUEST
            ? Db.CreateCommand("SELECT * FROM issues WHERE id = @issue_id AND customer_email = @customer_email")
            : Db.CreateCommand("SELECT * FROM issues WHERE id = @issue_id AND company_id = @company_id");

        cmd.Parameters.AddWithValue("@issue_id", issueId);
        Sender sender;
        
        if (user.Role == Role.GUEST)
        {
            cmd.Parameters.AddWithValue("@customer_email", user.Username);
            sender = Sender.CUSTOMER;
        }   
        else {
            cmd.Parameters.AddWithValue("@company_id", user.CompanyId);
            sender = Sender.SUPPORT;
        } 
        
        var reader = await cmd.ExecuteScalarAsync();
        if (reader == null)
        {
            return Results.Conflict(new { message = "You dont have access to post a message to this issue." });
        }
        
        await using var cmd2 = Db.CreateCommand("INSERT INTO messages (issue_id, message, sender, username, time) VALUES (@issue_id, @message, @sender::sender, @username, current_timestamp)");
        cmd2.Parameters.AddWithValue("@issue_id", issueId);
        cmd2.Parameters.AddWithValue("@message", createMessageRequest.Message);
        cmd2.Parameters.AddWithValue("@sender", sender);
        cmd2.Parameters.AddWithValue("@username", user.Username);
        
        try
        {
            var reader2 = await cmd2.ExecuteNonQueryAsync();
            if (reader2 == 1)
            {
                return Results.Ok(new { message = "Message was created successfully." });
            }
            else
            {
                return Results.Conflict(new { message = "Query executed but something went wrong." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message); 
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }
    }

    private string IssueCreatedMessage(string companyName, string message, string title, string chatId)
    {
        return $"<h1>{companyName}</h1>" +
               $"<br> <p>Tack för att du kontaktade oss!</p>" +
               "<p>Vi har tagit emot dit meddelande: </p>" +
               $"<br> <p><i>{message}</i></p> <br>" +
               $"<p>Vi har skapat ett chatt-rum där du kan prata direkt med en av våra kundtjänstmedarbetare angående ditt ärende <strong>{title}</strong>.</p>" +
               $"<p>För att ansluta till chatten, <a href='http://localhost:5173/chat/{chatId}'> klicka på denna länken.</a></p>" +
               $"<br> <br> <p>Vänliga hälsningar,</p>" +
               $"<p><strong>{companyName}</strong> kundtjänst.<br>";
    }
}

public class IssueRequest
{
    public string Email { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}