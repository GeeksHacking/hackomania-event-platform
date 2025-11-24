using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<HackOMania_Api>("api");

builder.AddJavaScriptApp("app", "../HackOMania.WebApp")
  .WaitFor(api)
  .WithReference(api)
  .WithHttpEndpoint(port: 3000, env: "PORT")
  .WithPnpm();

builder.Build().Run();
