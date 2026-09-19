using Empresa.Data;
using Empresa.Data.Repositories;

var builder = Host.CreateApplicationBuilder(args);

// DbSession (Scoped) — permite criar conexões dentro do Worker
builder.Services.AddScoped<DbSession>();

// Worker Service
builder.Services.AddHostedService<Empresa.Worker.Worker>();

var host = builder.Build();
host.Run();