using KiteConnect;
using System.Diagnostics;
using System.Net;

public class LoginManager
{
    private Kite _kite;
    private HttpListener listener;
    private CancellationTokenSource cancellationTokenSource;

    public string AccessToken = string.Empty;

    public EventHandler OnLogin { get; set; }

    public LoginManager(Kite kite)
    {
        _kite = kite;
        listener = new HttpListener();
        string loginUrl = kite.GetLoginURL();
        Process.Start(new ProcessStartInfo(loginUrl) { UseShellExecute = true });
        _ = StartServerAsync();
    }

    private async Task StartServerAsync()
    {
        cancellationTokenSource = new CancellationTokenSource();
        listener = new HttpListener();
        listener.Prefixes.Add("http://0.0.0.0:8080/");

        try
        {
            listener.Start();
            Console.WriteLine("Listening for requests...");
            await ListenForRequestsAsync(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            //MessageBox.Show($"Server failed to start: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ListenForRequestsAsync(CancellationToken token)
    {
        try
        {
            while (listener.IsListening && !token.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync();
                var queryParams = context.Request.QueryString;

                string status = queryParams["status"];
                AccessToken = queryParams["request_token"];

                if (status == "success" && !string.IsNullOrEmpty(AccessToken))
                {
                    await SendResponseAsync(context, "Logged in to Kite");

                    // Process login and fetch data
                    await ProcessLoginAsync(AccessToken);

                    // Stop listener after successful login
                    listener.Stop();
                    cancellationTokenSource.Cancel();
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
            listener.Close();
        }
    }
    private async Task SendResponseAsync(HttpListenerContext context, string message)
    {
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
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
