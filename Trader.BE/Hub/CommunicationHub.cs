using KiteConnect;
using Microsoft.AspNetCore.SignalR;

public class CommunicationHub : Hub
{
    // Define the delegate type for handling messages
    public delegate Task MessageReceivedHandler(string message);

    // Define the event based on the delegate
    public static event MessageReceivedHandler OnMessageReceived;

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
        await OnMessageReceived.Invoke("asd");
        await Clients.Caller.SendAsync("OnRefresh", url);
    }
}