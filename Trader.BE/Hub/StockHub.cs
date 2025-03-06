using Microsoft.AspNetCore.SignalR;

public class StockHub : Hub
{
    private IntradayServer _intradayServer;

    public StockHub(IntradayServer intradayServer)
    {
        _intradayServer = intradayServer;
    }

    public async Task SendStockUpdate(string scrip, int quantity, decimal factor, decimal price)
    {
        await Clients.All.SendAsync("ReceiveStockUpdate", scrip, quantity, factor, price);
    }

    public async Task SendLoginURL(string scrip, int quantity, decimal factor, decimal price)
    {
        await Clients.All.SendAsync("SendLoginURL", scrip, quantity, factor, price);
    }

    public async Task Refresh()
    {
        var url = _intradayServer.kite.GetLoginURL();
        await Clients.Caller.SendAsync("OnRefresh", url);
    }
}