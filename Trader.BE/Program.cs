using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:8080/");

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.SetIsOriginAllowed(_ => true)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
});

// Add SignalR as a singleton
builder.Services.AddSignalR();
builder.Services.AddSingleton<StockHub>();
builder.Services.AddSingleton<IntradayServer>();

builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowAll");

app.MapHub<StockHub>("/stockHub"); // SignalR Hub endpoint

app.Run();