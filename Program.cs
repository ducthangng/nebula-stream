using Nebula.Handlers;
using Nebula.web;
using Nebula.ECR;

DotNetEnv.Env.Load();

var registry = new Registry();
var cli = new CLIHandler(registry);

// CLI MODE
if (args.Length > 0)
{
    return await cli.RunAsync(args);
}

// WEB MODE
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddApplicationPart(typeof(DocsController).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nebula API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapControllers();
app.Run();

return 0;