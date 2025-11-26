using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var appFrontendUrl = builder.AddParameter("app-frontend-url", "http://localhost:3000");

var githubClientId = builder.AddParameter("github-client-id");
var githubClientSecret = builder.AddParameter("github-client-secret");

var api = builder
    .AddProject<HackOMania_Api>("api")
    .WithEnvironment("App:FrontendUrl", appFrontendUrl)
    .WithEnvironment("GitHub:ClientId", githubClientId)
    .WithEnvironment("GitHub:ClientSecret", githubClientSecret);

builder
    .AddJavaScriptApp("app", "../HackOMania.WebApp")
    .WithPnpm()
    .WithReference(api)
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WaitFor(api);

builder.Build().Run();
