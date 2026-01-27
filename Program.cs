using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nebula.Handlers;
using Nebula.web;

// CLI MODE
if (args.Length > 0)
{
    return await CLIHandler.RunAsync(args);
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
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nebula API V1");
        c.RoutePrefix = string.Empty; 
    });
}

app.MapControllers(); 
app.Run();

return 0;