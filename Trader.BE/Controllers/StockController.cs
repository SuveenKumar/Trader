using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/stocks")]
public class StockController : ControllerBase
{
    [HttpPost("place-order")]
    public IActionResult PlaceOrder()
    {
        return Ok(new { message = "Order placed successfully!" });
    }
}
