var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Lisha_Api>("lisha-api");

builder.AddProject<Projects.Lisha_Blazor>("lisha-blazor");

builder.Build().Run();
