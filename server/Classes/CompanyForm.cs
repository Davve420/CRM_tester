namespace server.Classes;

public class CompanyForm
{
    public string CompanyName { get; set; }
    public List<string> Subjects { get; set; }

    public CompanyForm(string companyName, List<string> subjects)
    {
        CompanyName = companyName;
        Subjects = subjects;
    }
}