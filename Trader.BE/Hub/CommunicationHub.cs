using KiteConnect;
using Microsoft.AspNetCore.SignalR;

public class CommunicationHub : Hub
{
    public delegate Task OnRefreshReceived(string message);

    private OrderManager _orderManager;
    private LoginManager _loginManager;

    public CommunicationHub(OrderManager orderManager, LoginManager loginManager)
    {
        _loginManager = loginManager;
        _orderManager = orderManager;
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
        var url = _orderManager.GetLoginURL();
        await OnRefreshReceived.Invoke();
        await Clients.Caller.SendAsync("OnRefresh", url);
    }
}