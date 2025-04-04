# CRM-Project 

## Setup

### Install dependencis
- In the terminal, open client and run "npm install". 
- In the terminal, open server and run "dotnet restore".

### Database

- Create an PostgreSQL database, open a query console, copy the script from "Database-Script.sql" and run it.
- Go in to the Database.cs and add your PostgreSQL connection information. 

Example
```c#
private readonly string _host = "localhost";
private readonly string _port = "5432";
private readonly string _username = "postgres";
private readonly string _password = "password";
private readonly string _database = "crm-site";
```

### Email

- In server, open the appsettings.json and configure the "Email"-object. You need to sett the email and password for the "Email".  

Example
```txt
"Email": {
    "SmtpServer": "smtp.gmail.com", // If your not using a gmail, you need to configure this.
    "SmtpPort": 587, 
    "FromEmail": "your-email",
    "Password": "your-password"
  }
```

