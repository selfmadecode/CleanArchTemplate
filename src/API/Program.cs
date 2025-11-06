using API;
using API.Extension;
using Application;
using Application.Models;
using Infrastructure;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.ConfigureSwagger();
builder.Services.AddAuthorizationPolicy();
builder.Services.AddCors(options =>
{
    var corsUrls = builder.Configuration["CORSAllowedOrigins"]
              .Split(",", StringSplitOptions.RemoveEmptyEntries)
                     .ToArray();
    options.AddPolicy("CorsPolicy",
    builder =>
    {
        builder.WithOrigins(corsUrls)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration.GetValue<string>("FilePath"))));
builder.Services.Configure<SmtpConfigSettings>(builder.Configuration.GetSection("SmtpConfig"));
builder.Services.Configure<EmailLink>(options =>
builder.Configuration.GetSection(nameof(EmailLink)).Bind(options));
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    await app.SeedDatabaseAsync();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CleanArchTemplate");
});
app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
