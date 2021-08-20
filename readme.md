# Server Setup Tester

This is a simple web server configuration tester. It will check various aspects for correct configuration.

## What is tested

Each of the following can be configured and enabled or disabled in the `appsettings.json` config file.

* **Database connection** - A connection is attempted to the SQL Server database using the credentials provided.
* **Email SMTP configuration** - An email is sent from the web server to the email address provided.
* **Firewall settings** - A connection is attempted for each required external service.
