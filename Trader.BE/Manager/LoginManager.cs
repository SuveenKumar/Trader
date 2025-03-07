using KiteConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.SignalR;


public class LoginManager
{
    private Kite _kite;
    private HttpListener _listener;
    private CancellationTokenSource _cancellationTokenSource;

    public string AccessToken = string.Empty;

    public EventHandler OnLogin { get; set; }
    public async Task StartServerAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:8080/");

        try
        {
            _listener.Start();
            Console.WriteLine("Listening for requests...");
            await ListenForRequestsAsync(_cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
           // MessageBox.Show($"Server failed to start: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ListenForRequestsAsync(CancellationToken token)
    {
        try
        {
            while (_listener.IsListening && !token.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync();
                var queryParams = context.Request.QueryString;

                string status = queryParams["status"];
                AccessToken = queryParams["request_token"];

                if (status == "success" && !string.IsNullOrEmpty(AccessToken))
                {
                    await SendResponseAsync(context, "Logged in to Kite");

                    // Process login and fetch data
                    await ProcessLoginAsync(AccessToken);

                    // Stop listener after successful login
                    _listener.Stop();
                    _cancellationTokenSource.Cancel();
                    break;
                }
                else
                {
                    await SendResponseAsync(context, "Login failed");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during request processing: {ex.Message}");
        }
        finally
        {
            _listener.Close();
        }
    }
    private async Task SendResponseAsync(HttpListenerContext context, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }

    private async Task ProcessLoginAsync(string requestToken)
    {
        try
        {
            var user = await Task.Run(() => _kite.GenerateSession(requestToken, TradingConstants.APISECRET));
            AccessToken = user.AccessToken;
            _kite.SetAccessToken(user.AccessToken);
            OnLogin?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //});
        }
    }
}
