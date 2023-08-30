# Server Checkup

This is a simple web server configuration tester. It will check various server aspects for correct configuration.

## What is tested

Each of the following can be configured and enabled or disabled in the web app settings file (see "appsettings-example.json" for format and options).

* **Email SMTP configuration** - An email is sent from the web server to your work login account.
* **Database connection** - A connection is attempted to the SQL Server database using the credentials provided.
* **Database email configuration** - A connection is attempted to the SQL Server database configured and the `msdb.dbo.sp_send_dbmail` stored procedure is called to send an email from the database.
* **External services** - A connection is attempted for each specified external service.
* **Installed .NET framework versions**
