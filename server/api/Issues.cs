using System.Text.Json;
using Npgsql;
using server.Authorization;
using server.Classes;
using server.Enums;
using server.Records;
using server.Services;

namespace server.api;

public class Issues
{
    private NpgsqlDataSource Db;
    public Issues(WebApplication app, NpgsqlDataSource db, string url)
    {
        Db = db;
        url += "/issues";
        
        app.MapGet(url, (Delegate)GetIssueByCompany).RoleAuthorization(Role.USER,Role.ADMIN);
        app.MapGet(url + "/{issueId}", GetIssue).RoleAuthorization(Role.GUEST,Role.USER,Role.ADMIN);
        app.MapPut(url + "/{issueId}/state", UpdateIssueState).RoleAuthorization(Role.USER,Role.ADMIN);
        app.MapGet(url + "/{issueId}/messages", GetMessages).RoleAuthorization(Role.GUEST,Role.USER,Role.ADMIN);
        app.MapPost(url + "/{issueId}/messages", CreateMessage).RoleAuthorization(Role.GUEST,Role.USER,Role.ADMIN);;
        app.MapPost(url + "/create/{companyName}", CreateIssue);
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
    
    private async Task<IResult> CreateIssue(string companyName, CreateIssueRequest createIssueRequest, IEmailService email)
    {
        await using var cmd = Db.CreateCommand("SELECT * FROM companies WHERE name = @company_name");
        cmd.Parameters.AddWithValue("@company_name", companyName);

        try
        {
            var companyId = await cmd.ExecuteScalarAsync();
            if (companyId is null)
            {
                return Results.NotFound(new { message = "No company found." });
            }
        
            
            await using var cmd2 = Db.CreateCommand("INSERT INTO issues (company_id, customer_email, title, subject, state, created) VALUES (@company_id, @customer_email, @title, @subject, 'NEW', current_timestamp) RETURNING id;");
            cmd2.Parameters.AddWithValue("@company_id", companyId);
            cmd2.Parameters.AddWithValue("@customer_email", createIssueRequest.Email);
            cmd2.Parameters.AddWithValue("@title", createIssueRequest.Title);
            cmd2.Parameters.AddWithValue("@subject", createIssueRequest.Subject);

            var issuesId = await cmd2.ExecuteScalarAsync();
            if (issuesId is not null)
            {
                await using var cmd3 = Db.CreateCommand("INSERT INTO messages (issue_id, message, sender, username, time) VALUES (@issue_id, @message, 'CUSTOMER', @username, current_timestamp)");
                cmd3.Parameters.AddWithValue("@issue_id", issuesId);
                cmd3.Parameters.AddWithValue("@message", createIssueRequest.Message);
                cmd3.Parameters.AddWithValue("@username", createIssueRequest.Email);
                    
                try
                {
                    int rowsAffected = await cmd3.ExecuteNonQueryAsync();
                    if (rowsAffected == 1)
                    {
                        await email.SendEmailAsync(createIssueRequest.Email, 
                            $"{companyName} - ISSUE: {createIssueRequest.Title}", 
                            IssueCreatedMessage(companyName, 
                                createIssueRequest.Message, 
                                createIssueRequest.Title,
                                issuesId.ToString()));
                        return Results.Ok(new { message = "Issue was created successfully." });
                    }
                    else
                    {
                        return Results.Conflict(new { message = "Query executed but something went wrong." });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await using var cmd4 = Db.CreateCommand("DELETE FROM issues WHERE id = @issue_id;");
                    cmd4.Parameters.AddWithValue("@issue_id", issuesId);
                    await cmd4.ExecuteNonQueryAsync();
                        
                    return Results.Conflict(new { message = "Issue was created, but something went wrong during the process, so the issue has been deleted." });
                }
            }
            else
            {
                return Results.Json(new { message = "Something went wrong." }, statusCode: 500);   
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