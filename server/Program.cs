using Npgsql;
using server;
using server.api;
using server.Config;
using server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

Database database = new Database();
NpgsqlDataSource db = database.Connection();

var emailSettings = builder.Configuration.GetSection("Email").Get<EmailSettings>();
if (emailSettings != null)
{
    builder.Services.AddSingleton(emailSettings);
}
else
{
    throw new InvalidOperationException("Email settings are not configured properly.");
}
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseSession();

String url = "/api";

new ServerStatus(app, db, url);
new Login(app, db, url);
new Users(app, db, url);
new Issues(app, db, url);
new Forms(app, db, url);
new Companies(app, db, url);

await app.RunAsync();
