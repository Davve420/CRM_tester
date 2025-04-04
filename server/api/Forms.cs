using System.Text.Json;
using Npgsql;
using server.Authorization;
using server.Classes;
using server.Enums;
using server.Records;

namespace server.api;

public class Forms
{
    private NpgsqlDataSource Db;
    public Forms(WebApplication app, NpgsqlDataSource db, string url)
    {
        Db = db;
        url += "/forms";
        
        app.MapGet(url + "/{companyName}", GetCompanyForm);
        app.MapGet(url + "/subjects", (Delegate)GetFormSubjects).RoleAuthorization(Role.USER, Role.ADMIN);
        app.MapPost(url + "/subjects", CreateSubject).RoleAuthorization(Role.ADMIN);
        app.MapPut(url + "/subjects", UpdateSubject).RoleAuthorization(Role.ADMIN);
        app.MapDelete(url + "/subjects/{subjectName}", DeleteSubject).RoleAuthorization(Role.ADMIN);
    }
    
    private async Task<IResult> GetCompanyForm(string companyName)
    {
        await using var cmd = Db.CreateCommand("SELECT * FROM companies WHERE name = @company_name");
        cmd.Parameters.AddWithValue("@company_name", companyName);
    
        try
        {
            var reader = await cmd.ExecuteScalarAsync();
            if (reader is not null)
            {
                await using var cmd2 = Db.CreateCommand("SELECT name FROM subjects WHERE company_id = @company_id ORDER BY id");
                cmd2.Parameters.AddWithValue("@company_id", (Int32) reader);

                await using (var reader2 = await cmd2.ExecuteReaderAsync())
                {
                    List<String> formSubjects = new List<string>();
                    while (await reader2.ReadAsync())
                    {
                        formSubjects.Add(reader2.GetString(0));
                    }

                    if (formSubjects.Count == 0)
                    {
                        return Results.NotFound(new { message = "No subjects was found." });
                    }
                    
                    return Results.Ok(new {company_info = new CompanyForm(companyName, formSubjects)});
                }
            }else
            {
                return Results.NotFound(new { message = "No company was found." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }
    }

    private async Task<IResult> GetFormSubjects(HttpContext context)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));
        
        await using var cmd = Db.CreateCommand("SELECT * FROM subjects WHERE company_id = @company_id ORDER BY id");
        cmd.Parameters.AddWithValue("@company_id", user.CompanyId);

        try
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                List<String> formSubjects = new List<string>();
                while (reader.Read())
                {
                    formSubjects.Add(reader.GetString(reader.GetOrdinal("name")));
                }
                
                if (formSubjects.Count > 0)
                {
                    return Results.Ok(new { subjects = formSubjects });
                }
                else
                {
                    return Results.NotFound(new { message = "No subjects were found." });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }
    }

    private async Task<IResult> CreateSubject(HttpContext context, CreateSubjectRequest createSubjectRequest)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));
        
        await using var cmd = Db.CreateCommand("INSERT INTO subjects (company_id, name) VALUES (@company_id, @name)");
        cmd.Parameters.AddWithValue("@company_id", user.CompanyId);
        cmd.Parameters.AddWithValue("@name", createSubjectRequest.Name);
        
        
        try
        {
            var reader = await cmd.ExecuteNonQueryAsync();
            if (reader == 1)
            {
                return Results.Ok(new { message = "Subject was successfully created." });
            }
            else
            {
                return Results.Conflict(new { message = $"Query was executed, but {reader} rows was effected." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }
    }
    
    private async Task<IResult> UpdateSubject(HttpContext context, UpdateSubjectRequest updateSubjectRequest)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));
        
        await using var cmd = Db.CreateCommand("UPDATE subjects SET name = @new_name WHERE company_id = @company_id AND name = @old_name");
        cmd.Parameters.AddWithValue("@new_name", updateSubjectRequest.NewName);
        cmd.Parameters.AddWithValue("@company_id", user.CompanyId);
        cmd.Parameters.AddWithValue("@old_name", updateSubjectRequest.OldName);
    
        try
        {
            var reader = await cmd.ExecuteNonQueryAsync();
            if (reader == 1)
            {
                return Results.Ok(new { message = "Subject was updated." });
            }
            else
            {
                return Results.Conflict(new { message = $"Query was executed, but {reader} rows was effected." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }
    }
    
    private async Task<IResult> DeleteSubject(string subjectName, HttpContext context)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));
        
        await using var cmd = Db.CreateCommand("DELETE FROM subjects WHERE company_id = @company_id AND name = @name");
        cmd.Parameters.AddWithValue("@company_id", user.CompanyId);
        cmd.Parameters.AddWithValue("@name", subjectName);
    
        try
        {
            var reader = await cmd.ExecuteNonQueryAsync();
            if (reader == 1)
            {
                return Results.Ok(new { message = "Subject was deleted." });
            }
            else
            {
                return Results.Conflict(new { message = $"Query was executed, but {reader} rows was effected." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }
    }
    
}