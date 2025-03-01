using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Enable CORS (Allow frontend to connect to backend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.SetIsOriginAllowed(_ => true) // Allows all origins
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()); // Required for SignalR
});

// Add SignalR and Controllers
builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowAll"); // Apply CORS policy
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<StockHub>("/stockHub"); // SignalR Hub endpoint

app.Run();

public class StockHub : Hub
{
    public async Task SendStockUpdate(string scrip, int quantity, decimal factor, decimal price)
    {
        await Clients.All.SendAsync("ReceiveStockUpdate", scrip, quantity, factor, price);
    }
}
