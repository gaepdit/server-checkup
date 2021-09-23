# Server Setup Tester

This is a simple web server configuration tester. It will check various server aspects for correct configuration.

## What is tested

Each of the following can be configured and enabled or disabled in the `appsettings.json` config file.

* **Database connection** - A connection is attempted to the SQL Server database using the credentials provided.
* **Email SMTP configuration** - An email is sent from the web server to the email address provided. (If no emails are
  specified in the settings file or on the command line, an email will be requested at runtime.)
* **Firewall settings** - A connection is attempted for each specified external service.

## Command line options

Run `CheckServerSetup --help` for a list of all command-line options.

If the option `--email <email address>` is used, this email will be added to any specified in the settings file.