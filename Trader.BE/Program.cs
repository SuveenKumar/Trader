using KiteConnect;
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
builder.Services.AddSingleton<CommunicationHub>();
builder.Services.AddSingleton<OrderManager>();
builder.Services.AddSingleton<LoginManager>();
builder.Services.AddSingleton(provider =>
{
    return new Kite(TradingConstants.APIKEY);  // Register the service with the apiKey
});

//builder.Services.AddControllers();
//builder.Services.AddHostedService<TraderAC>(); // This will run on startup
var app = builder.Build();
app.UseCors("AllowAll");

app.MapHub<CommunicationHub>("/stockHub"); // SignalR Hub endpoint

app.Run();