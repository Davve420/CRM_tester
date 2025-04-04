namespace server.Classes;

public class Message
{
    public String Text { get; set; }
    public String Sender { get; set; }
    public String Username { get; set; }
    public DateTime Time{ get; set; }

    public Message(string text, string sender, string username, DateTime time)
    {
        Text = text;
        Sender = sender;
        Username = username;
        Time = time;
    }
}