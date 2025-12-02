using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using HackOMania.Api.Options;
using HackOMania.Api.Workers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOptions<AppOptions>().Bind(builder.Configuration.GetSection("App"));
builder.Services.AddOptions<GitHubOptions>().Bind(builder.Configuration.GetSection("GitHub"));

builder.Services.AddSingleton<ISqlSugarClient>(s =>
{
    var sqlSugar = new SqlSugarScope(
        new ConnectionConfig()
        {
            DbType = DbType.Sqlite,
            ConnectionString = "DataSource=app.db",
            IsAutoCloseConnection = true,
        },
        db => { }
    );
    return sqlSugar;
});

builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseInMemoryDatabase("Db");
    options.UseOpenIddict();
});

builder
    .Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<DbContext>();
    })
    .AddClient(options =>
    {
        options.AllowAuthorizationCodeFlow();
        options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
        options.UseAspNetCore().EnableRedirectionEndpointPassthrough();
        options.UseSystemNetHttp();

        options
            .UseWebProviders()
            .AddGitHub(github =>
            {
                var githubOptions = builder
                    .Configuration.GetSection("GitHub")
                    .Get<GitHubOptions>()!;

                github
                    .SetClientId(githubOptions.ClientId)
                    .SetClientSecret(githubOptions.ClientSecret)
                    .SetRedirectUri("callback/login/github");
            });
    });

builder
    .Services.AddAuthenticationCookie(validFor: TimeSpan.FromDays(7))
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            builder.Environment.IsDevelopment()
                ? ["http://localhost:3000"]
                : ["https://hackomania.geekshacking.org"]
        );

        policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(options =>
{
    options.DocumentSettings = settings =>
    {
        settings.Title = "HackOMania Event Platform API";
        settings.DocumentName = "api";
        settings.Version = "v1";
    };
});

builder.Services.AddHostedService<DatabaseInitBackgroundService>();

var app = builder.Build();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen(options => options.Path = "/openapi/{documentName}.json");

app.Run();
