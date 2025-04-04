using System.Data;
using System.Text.Json;
using Npgsql;
using server.Authorization;
using server.Classes;
using server.Enums;
using server.Records;

namespace server.api;

public class Users
{
    private NpgsqlDataSource Db;
    public Users(WebApplication app, NpgsqlDataSource db, string url)
    {
        Db = db;
        url += "/users";
        
        app.MapGet(url + "/bycompany", (Delegate)GetEmployeesByCompany).RoleAuthorization(Role.ADMIN);
        app.MapPost(url + "/admin", CreateAdmin);
        app.MapPost(url + "/create", CreateEmployee).RoleAuthorization(Role.ADMIN);
        app.MapPut(url + "/{userId}", UpdateUser).RoleAuthorization(Role.ADMIN);
        app.MapDelete(url + "/{userId}", DeleteUser).RoleAuthorization(Role.ADMIN);
    }
    
    async Task<IResult> GetEmployeesByCompany(HttpContext context)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));
        
        List<Employee> employeesList = new List<Employee>();
        await using var cmd = Db.CreateCommand("SELECT * FROM users_with_company WHERE company_name = @company_name");
        cmd.Parameters.AddWithValue("@company_name", user.Company);

        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    employeesList.Add(new Employee(
                        reader.GetInt32(reader.GetOrdinal("user_id")),
                        reader.GetString(reader.GetOrdinal("username")),
                        reader.IsDBNull(reader.GetOrdinal("firstname")) ? String.Empty : reader.GetString(reader.GetOrdinal("firstname")),
                        reader.IsDBNull(reader.GetOrdinal("lastname")) ? String.Empty :reader.GetString(reader.GetOrdinal("lastname")),
                        reader.GetString(reader.GetOrdinal("email")),
                        Enum.Parse<Role>(reader.GetString(reader.GetOrdinal("role")))
                    ));
                } 
                return Results.Ok(new { employees = employeesList });    
            }
        }
        return Results.NoContent();
    }

    async Task<IResult> CreateAdmin(RegisterRequest registerRequest)
    {
        await using var cmd = Db.CreateCommand("INSERT INTO companies (name) VALUES (@company) RETURNING id, name;");
        cmd.Parameters.AddWithValue("@company", registerRequest.Company);
        
        try
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    await using var cmd2 = Db.CreateCommand("INSERT INTO users (username, password, role, email, company) VALUES (@username, @password, 'ADMIN', @email, @company_id);");
                    cmd2.Parameters.AddWithValue("@username", registerRequest.Username);
                    cmd2.Parameters.AddWithValue("@email", registerRequest.Email);
                    cmd2.Parameters.AddWithValue("@password", registerRequest.Password);
                    cmd2.Parameters.AddWithValue("@company_id", reader.GetInt32(reader.GetOrdinal("id")));
                    
                    try
                    {
                        await using (var reader2 = await cmd2.ExecuteReaderAsync())
                        {
                            if (reader2.RecordsAffected == 1)
                            {
                                return Results.Ok(new { message = "User registered." });
                            }
                        }
                    }
                    catch
                    {
                        await using var cmd3 = Db.CreateCommand("DELETE FROM companies WHERE name = @company;");
                        cmd3.Parameters.AddWithValue("@company", registerRequest.Company);
                        await cmd3.ExecuteNonQueryAsync();
                        
                        return Results.Conflict(new { message = "User already exists." });
                    }
                }
            }
        }
        catch
        {
            return Results.Conflict(new { message = "Company already exists." });
        }
        
        return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
    }

    async Task<IResult> CreateEmployee(HttpContext context, CreateEmployeeRequest createEmployeeRequest)
    {
        try
        {
            var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));
            
            await using var cmd = Db.CreateCommand("SELECT * FROM companies WHERE name = @company");
            cmd.Parameters.AddWithValue("@company", user.Company);
        
            var reader = await cmd.ExecuteScalarAsync();
            if(reader != null)
            {
                await using var cmd2 = Db.CreateCommand("INSERT INTO users (firstname, lastname, username, password, role, email, company) VALUES (@firstname, @lastname, @username, @password, @role::role, @email, @company_id);");
                cmd2.Parameters.AddWithValue("@firstname", createEmployeeRequest.Firstname);
                cmd2.Parameters.AddWithValue("@lastname", createEmployeeRequest.Lastname);
                cmd2.Parameters.AddWithValue("@username", createEmployeeRequest.Firstname + "_" + createEmployeeRequest.Lastname);
                cmd2.Parameters.AddWithValue("@password", createEmployeeRequest.Password);
                cmd2.Parameters.AddWithValue("@role", Enum.Parse<Role>(createEmployeeRequest.Role).ToString());
                cmd2.Parameters.AddWithValue("@email", createEmployeeRequest.Email);
                cmd2.Parameters.AddWithValue("@company_id", reader);
                
                try
                {
                    int rowsAffected = await cmd2.ExecuteNonQueryAsync();
                    if (rowsAffected == 1)
                    {
                        return Results.Ok(new { message = "User registered." });
                    }
                    else
                    {
                        return Results.Conflict(new { message = "Query executed but something went wrong." });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return Results.Conflict(new { message = "User already exists" });
                }
            }
            else
            {
                return Results.NotFound(new { message = "Companies not found." });
            }
            
        }
        catch
        {
            return Results.Json(new { message = "Something went wrong." }, statusCode: 500);
        }
    }

    async Task<IResult> UpdateUser(int userId, HttpContext context, UpdateUserRequest updateUserRequest)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));
        
        await using var cmd = Db.CreateCommand("UPDATE users SET firstname = @firstname, lastname = @lastname, email = @email, role = @role::role WHERE id = @user_id AND company = @company_id;");
        cmd.Parameters.AddWithValue("@firstname", updateUserRequest.Firstname);
        cmd.Parameters.AddWithValue("@lastname", updateUserRequest.Lastname);
        cmd.Parameters.AddWithValue("@email", updateUserRequest.Email);
        cmd.Parameters.AddWithValue("@role", Enum.Parse<Role>(updateUserRequest.Role).ToString());
        cmd.Parameters.AddWithValue("@user_id", userId);
        cmd.Parameters.AddWithValue("@company_id", user.CompanyId);

        try
        {
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 1)
            {
                return Results.Ok(new { message = "User updated successfully." });
            }
            else
            {
                return Results.Conflict(new { message = "Query executed but something went wrong." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Conflict(new { message = "User update failed." });
        }
    }

    async Task<IResult> DeleteUser(int userId, HttpContext context)
    {
        var user = JsonSerializer.Deserialize<User>(context.Session.GetString("User"));
        
        await using var cmd = Db.CreateCommand("DELETE FROM users WHERE id = @user_id AND company = @company_id;");
        cmd.Parameters.AddWithValue("@user_id", userId);
        cmd.Parameters.AddWithValue("@company_id", user.CompanyId);
        
        try
        {
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 1)
            {
                return Results.Ok(new { message = "User was deleted successfully." });
            }
            else if (rowsAffected == 0)
            {
                return Results.NotFound(new { message = "No user was found." });
            }
            else
            {
                return Results.Conflict(new { message = "Query executed but something went wrong." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.Conflict(new { message = "Query was not executed." });
        }
    }
    
}