## CleanArchTemplate (.NET 9 Clean Architecture)

[![License](https://img.shields.io/github/license/selfmadecode/CleanArchTemplate.svg?style=flat)](LICENSE)
[![Build](https://img.shields.io/github/actions/workflow/status/selfmadecode/CleanArchTemplate/build.yml?branch=master&style=flat&logo=github)](https://github.com/selfmadecode/CleanArchTemplate/actions)
[![.NET ≥ 8.0](https://img.shields.io/badge/.NET-%E2%89%A58.0-512BD4?logo=dotnet&style=flat)](https://dotnet.microsoft.com/)
[![GitHub issues](https://img.shields.io/github/issues/selfmadecode/CleanArchTemplate.svg?style=flat)](https://github.com/selfmadecode/CleanArchTemplate/issues)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat)](https://github.com/selfmadecode/CleanArchTemplate/pulls)

## About the Project

**CleanArchTemplate** is a fully featured **.NET 9 Clean Architecture** boilerplate project built for rapid enterprise application development.  
It provides a ready-to-use foundation with modern best practices and modular layers, so you can quickly rename and launch new projects.

### ✨ Key Features

- ✅ **.NET 9 Clean Architecture** pattern  
- ✅ **Entity Framework Core** with `DbContext`  
- ✅ **Authentication & Authorization** using **Roles** and **Policies**  
- ✅ **JWT with Refresh Tokens**  
- ✅ **File Upload Support**
- ✅ **Email Sending via SendGrid and Mailgun** (requires API keys)  
- ✅ **Email Templates (Account Registration, etc)** ready to use  
- ✅ **Dependency Injection (DI) Registration**  
- ✅ **Logging with NLog**  
- ✅ **Swagger/OpenAPI** documentation  
- ✅ **Ready to use out-of-the-box** after renaming  
- ✅ Organized in **four projects**:
  - `API`
  - `Application`
  - `Domain`
  - `Infrastructure`
- ✅ Common base types such as:
  - `EntityBase` (as record)
  - `ApplicationUser`
  - DTOs

Use this project as a starting point for scalable, secure, and maintainable .NET applications.

---

# Rename Guide

If you want to create a new project based on this template (for example, rename **CleanArchTemplate** to **NewProjectName**), follow the steps below.

---

## Step 1 – Close the IDE

Close **Visual Studio**, **Rider**, or **VS Code** before renaming to avoid file locks.

---

## Step 2 – Rename the Solution and Folder

In **File Explorer**:

1. Rename the main project folder  
```
CleanArchTemplate → NewProjectName
```
2. Rename the solution file  
```
CleanArchTemplate.sln → NewProjectName.sln
```

---

## Step 3 – Update the Solution File

Open the `.sln` file in a text editor (Notepad or VS Code) and search for:

```
Project("{GUID}") = "CleanArchTemplate", "CleanArchTemplate\CleanArchTemplate.csproj", "{GUID}"
```
Replace **CleanArchTemplate** with your new project name:
```
Project("{GUID}") = "NewProjectName", "NewProjectName\NewProjectName.csproj", "{GUID}"
```

Save and close.

---

## ⚙️ Step 4 – Rename the Project Files (Optional)

If you wish, rename your `.csproj` files to match:

```

API.csproj               → NewProjectName.API.csproj
Application.csproj       → NewProjectName.Application.csproj
Infrastructure.csproj    → NewProjectName.Infrastructure.csproj
Domain.csproj            → NewProjectName.Domain.csproj

````

This step is optional; the template will still run with the original file names.

---

## 🧩 Step 5 – Update Namespaces and References

Search the entire solution for **`CleanArchTemplate`** and replace it with your new project name, e.g.:

```csharp
namespace CleanArchTemplate.API.Controllers
````

becomes

```csharp
namespace NewProjectName.API.Controllers
```

Also update any `<ProjectReference>` entries if you renamed `.csproj` files:

```xml
<ProjectReference Include="..\Application\Application.csproj" />
```

to

```xml
<ProjectReference Include="..\Application\NewProjectName.Application.csproj" />
```

---

## Step 6 – Update `.csproj` Properties

Inside each `.csproj` file, ensure the following reflect your new project name:

```xml
<PropertyGroup>
  <RootNamespace>NewProjectName</RootNamespace>
  <AssemblyName>NewProjectName</AssemblyName>
</PropertyGroup>
```

---

## Step 7 – Update App Settings and Database Name

Open `appsettings.json` in the API project and update your connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Database=NewProjectNameDB;..."
}
```

---

## Step 8 – Update Email and SMTP Configuration

In `appsettings.json`, update the following sections:

### **SmtpConfig**

Set your actual SMTP credentials:

```json
"SmtpConfig": {
  "Host": "smtp.yourdomain.com",
  "Port": 587,
  "Mail": "noreply@yourdomain.com",
  "Password": "your_smtp_password",
  "DisplayName": "Your App Name"
}
```

### **EmailLink**

Update the domain to match your new environment:

```json
"EmailLink": {
  "Domain": "https://newprojectname.com"
}
```

###  **CORSAllowedOrigins**

Add your allowed frontend domains:

```json
"CORSAllowedOrigins": [
  "https://newprojectname.com",
  "https://admin.newprojectname.com"
]
```

Then open the **Mail Service** class:

```
Application/Services/MailService.cs
```

Locate the method:

```csharp
SendEmailViaMailgun()
```

and update the Mailgun **domain** to match your configuration.

---

## Step 9 – Update Swagger Details

Go to:
`API/Extensions/ServiceCollectionExtensions.cs`

Find `ConfigureSwagger` and change the Swagger title, description, and version text from **CleanArchTemplate** to **NewProjectName**.

In `Program.cs`, update:

```csharp
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CleanArchTemplate");
});
```

to

```csharp
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NewProjectName");
});
```

---

## Step 10 – Update Emails and Usernames

Search for both `CleanArchTemplate` and `@cleanarchtemplate` and replace them with your new project name.

Example:

```
superadmin@cleanarchtemplate.com
```

becomes

```
superadmin@newprojectname.com
```

---

## Step 11 – Run Migrations

To run or add Entity Framework migrations:

1. In Visual Studio, **set the Infrastructure project as Startup Project**.
2. Open the Package Manager Console and run:

   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```

---

## Step 12 – Clean & Rebuild

After renaming:

```bash
dotnet clean
dotnet build
```

Run the API and confirm everything works correctly.

---

# Summary

| Area               | What to Change                                    |
| ------------------ | ------------------------------------------------- |
| Solution / Folder  | Rename to your new project name                   |
| Namespaces         | Replace `CleanArchTemplate` with `NewProjectName` |
| Emails / Usernames | Replace `@cleanarchtemplate`                      |
| SMTP Settings      | Update host, mail, password, display name, port   |
| Email Link         | Update domain                                     |
| CORS               | Update allowed origins                            |
| Mail Service       | Update Mailgun domain                             |
| Swagger            | Update title and endpoint                         |
| Database           | Update connection string name                     |
| Migrations         | Run with Infrastructure as startup project        |

---

 **That’s it!**
After completing these steps, your new project (**NewProjectName**) will be fully operational — complete with authentication, authorization, Swagger, file upload, EF Core, DI, NLog, and refresh tokens — all ready to go!

---

>  **Tip:** Commit your renamed project as a new GitHub repository to keep this template clean for future reuse.
