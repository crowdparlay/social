using CrowdParlay.Social.Application;
using CrowdParlay.Social.Application.Middlewares;
using CrowdParlay.Social.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options => options.SuppressMapClientErrors = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddApplication()
    .AddDatabase(builder.Configuration);

builder.Host.UseSerilog();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.AppSettings()
    .WriteTo.File("logs/CrowdParlay.Social.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRouting();
app.UseAuthorization();
app.UseCors();
app.MapControllers();

app.Run();