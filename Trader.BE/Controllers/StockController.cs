using KiteConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[ApiController]
[Route("api/stocks")]
public class StockController : ControllerBase
{
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IntradayServer _intradayServer;

    public StockController(IHubContext<StockHub> hubContext, IntradayServer intradayServer)
    {
        _hubContext = hubContext;
        _intradayServer = intradayServer;
        //AvailableScrips = TradingConstants.NIFTY100.ToList();
        //Stocks = new ObservableCollection<StockItem>();
        _intradayServer.ServerConnected -= OnServerConnected;
        _intradayServer.ServerDisconnected -= OnServerDisconnected;
        _intradayServer.ServerConnected += OnServerConnected;
        _intradayServer.ServerDisconnected += OnServerDisconnected;
    }

    //[HttpPost("place-order")]
    //public async Task<IActionResult> PlaceOrder()
    //{
    //    // Simulated stock data
    //        string scrip = "AAPL";
    //    int quantity = 10;
    //    decimal factor = 1.5m;
    //    decimal newPrice = 145.75m;

    //    // Notify all clients with stock update
    //    await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", scrip, quantity, factor, newPrice);

    //    return Ok(new { message = "Order placed successfully!", scrip, quantity, factor, newPrice });
    //}

    [HttpPost("login")]
    public async Task<IActionResult> RefreshPage()
    {
        Debug.WriteLine("RefreshPage");
        return Ok(new { message = "Logged In"});
    }

    //private ObservableCollection<StockItem> _stocks;
    //private bool _isServerConnected;
    //public bool IsServerConnected
    //{
    //    get => _isServerConnected;
    //    set
    //    {
    //        _isServerConnected = value;
    //        OnPropertyChanged(nameof(IsServerConnected));
    //    }
    //}
    //public ObservableCollection<StockItem> Stocks
    //{
    //    get => _stocks;
    //    set { _stocks = value; OnPropertyChanged(); }
    //}
    //public List<string> AvailableScrips { get; set; }

    private void OnServerDisconnected(object? sender, EventArgs e)
    {
        //IsServerConnected = false;
    }

    private async void OnServerConnected(object? sender, EventArgs e)
    {
        var holdings = _intradayServer.kite.GetHoldings();
        foreach (var holding in holdings)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", holding.TradingSymbol, holding.Quantity, holding.Price, holding.LastPrice);
        }
    }

    //public event PropertyChangedEventHandler? PropertyChanged;
    //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}

    //private void PlaceOpenOrderBtn_Click(object sender, RoutedEventArgs e)
    //{
    //    PlaceOpenOrderBtn.IsEnabled = false;
    //    _intradayServer.PlaceOpenOrder(Stocks);
    //}

    //private void AddRowBtn_Click(object sender, RoutedEventArgs e)
    //{
    //    Stocks.Add(new StockItem(_intradayServer.kite, _intradayServer.AvailableScrips)
    //    {
    //        SerialNo = Stocks.Count + 1,
    //        Scrip = "",
    //        Factor = 1.0123m,
    //    });
    //}
    //private void RemoveRowBtn_Click(object sender, RoutedEventArgs e)
    //{
    //    if (sender is FrameworkElement element && element.Tag is StockItem stock)
    //    {
    //        Stocks.Remove(stock);
    //        UpdateSerialNumbers();
    //    }
    //}

    //private void UpdateSerialNumbers()
    //{
    //    for (int i = 0; i < Stocks.Count; i++)
    //    {
    //        Stocks[i].SerialNo = i + 1;
    //    }
    //}
}



