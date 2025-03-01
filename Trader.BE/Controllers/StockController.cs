using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

[ApiController]
[Route("api/stocks")]
public class StockController : ControllerBase
{
    private readonly IHubContext<StockHub> _hubContext;

    public StockController(IHubContext<StockHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost("place-order")]
    public async Task<IActionResult> PlaceOrder()
    {
        // Simulated stock data
            string scrip = "AAPL";
        int quantity = 10;
        decimal factor = 1.5m;
        decimal newPrice = 145.75m;

        // Notify all clients with stock update
        await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", scrip, quantity, factor, newPrice);

        return Ok(new { message = "Order placed successfully!", scrip, quantity, factor, newPrice });
    }
}
