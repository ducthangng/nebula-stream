using Nebula.Handlers;
using Nebula.web;
using Nebula.ECR;
using Amazon.EKS;
using NebulaStream.CloudAbstractions;
using NebulaStream.Infrastructure.CloudProviders.AWS;

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

// standard logger config 
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// --- 2. Cấu hình AWS SDK ---
// Lấy Credentials và Region từ appsettings.json hoặc Environment Variables
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonEKS>();


// --- 3. Đăng ký Database (PostgreSQL) ---
// Chuỗi kết nối lấy từ appsettings.json: ConnectionStrings:DefaultConnection
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


builder.Services.AddScoped<IClusterRepository>(sp =>
{
    // Lấy Logger đã được hệ thống cấu hình sẵn ra
    var logger = sp.GetRequiredService<ILogger<PostgresClusterRepository>>();

    // Trả về instance của Repository
    return new PostgresClusterRepository(connectionString, logger);
});

// --- 4. Đăng ký Business Logic (SOLID) ---
// Đăng ký Cloud Orchestrator cho AWS
builder.Services.AddScoped<IKubernetesOrchestration, EksClusterOrchestrator>();

// Đăng ký Manager - Lớp điều phối chính cho Nebula Stream
builder.Services.AddScoped<ClusterManager>();


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
app.UseHttpsRedirection();
app.UseAuthorization();

app.Run();

return 0;