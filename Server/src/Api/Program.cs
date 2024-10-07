using System.Text.Json.Serialization;
using API.Configurations;
using API.Extensions;
using API.Middleware;
using Hangfire;


var builder = WebApplication.CreateBuilder(args);

builder.RegisterConfigurationOptions();
builder.RegisterServices();
builder.RegisterOptions();
builder.RegisterDbContext();
builder.RegisterHangfire();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddAuthentication()
    .AddJwtBearer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RevokedTokenMiddleware>();
app.MapControllers();
app.UseHangfireDashboard();

JobScheduler.RegisterJobs(); 

await app.RunAsync();