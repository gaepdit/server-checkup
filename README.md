# Server Checkup

This is a simple web server configuration tester. It will check various server aspects for correct configuration.

[![Georgia EPD-IT](https://raw.githubusercontent.com/gaepdit/gaepd-brand/main/blinkies/blinkies.cafe-gaepdit.gif)](https://github.com/gaepdit)
[![SonarCloud Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=gaepdit_server-checkup&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=gaepdit_server-checkup)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=gaepdit_server-checkup&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=gaepdit_server-checkup)

## What is tested

Each of the following can be configured and enabled or disabled in the web app settings file (see "appsettings-example.json" for format and options).

* **Email SMTP configuration** - An email is sent from the web server to your work login account.
* **Database connection** - A connection is attempted to the SQL Server or Oracle database using the credentials provided.
* **Database email configuration** - A connection is attempted to the SQL Server database configured and the `msdb.dbo.sp_send_dbmail` stored procedure is called to send an email from the database.
* **External services** - A connection is attempted for each specified external service.
* **Installed .NET framework versions**
