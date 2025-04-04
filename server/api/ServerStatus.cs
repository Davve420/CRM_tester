using Npgsql;

namespace server.api;

public class ServerStatus
{
    private NpgsqlDataSource Db;
    public ServerStatus(WebApplication app, NpgsqlDataSource db, string url)
    {
        Db = db;
        url += "/";
        
        app.MapGet(url, () => "Server is running!");
    }
}