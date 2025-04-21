using Npgsql;
using server;
using server.api;
using server.Config;
using server.Services;
using Microsoft.EntityFrameworkCore;
using server.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

Database database = new Database();
NpgsqlDataSource db = database.Connection();
/*
builder.Services.AddScoped<IEmailService, EmailService>();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
*/
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseSession();

String url = "/api";

new ServerStatus(app, db, url);
new Login(app, db, url);
new Users(app, db, url);
var issues = new Issues(app, db, url);
new Forms(app, db, url);
new Companies(app, db, url);

await app.RunAsync();

public class Issues
{
    public Issues(WebApplication app, NpgsqlDataSource db, string url)
    {
        throw new NotImplementedException();
    }
}
