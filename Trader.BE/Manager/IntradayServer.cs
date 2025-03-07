﻿using KiteConnect;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;


public class IntradayServer
{
    public List<KiteConnect.Instrument> AvailableScrips = new List<KiteConnect.Instrument>();
    decimal DefaultFactor = 1.0123m;

    public Kite kite;

    private Ticker ticker;
    public EventHandler ServerConnected;
    public EventHandler ServerDisconnected;

    private ObservableCollection<StockItem> _selectedStocks;

    public string AccessToken;

    public IntradayServer()
    {
        kite = new Kite(TradingConstants.APIKEY);
    }

    public async Task ProcessLoginAsync(string requestToken)
    {
        try
        {
            var user = await Task.Run(() => kite.GenerateSession(requestToken, TradingConstants.APISECRET));
            AccessToken = user.AccessToken;
            kite.SetAccessToken(user.AccessToken);
        }
        catch (Exception ex)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //});
        }
    }

    private void OnLoggedIn(object? sender, EventArgs e)
    {
        AvailableScrips = kite.GetInstruments().ToList();
        InitTicker();
    }

    private void InitTicker()
    {
        ticker = new Ticker(TradingConstants.APIKEY, AccessToken);
        SubscribeToTickerEvents();
    }

    private void SubscribeToTickerEvents()
    {
        ticker.OnTick += OnTick;
        ticker.OnReconnect += OnTickerReconnect;
        ticker.OnNoReconnect += OnTickerNoReconnect;
        ticker.OnError += OnTickerError;
        ticker.OnClose += OnTickerClose;
        ticker.OnConnect += OnTickerConnect;
        ticker.OnOrderUpdate += OnOrderUpdate;

        ticker.EnableReconnect(Interval: 5, Retries: 50);
        ticker.Connect();
    }

    private void OnTickerConnect()
    {
        ServerConnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnTickerClose()
    {
        Debug.WriteLine("OnTickerClose");
        ServerDisconnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnTickerError(string Message)
    {
        Debug.WriteLine("OnTickerError");
        ServerDisconnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnTickerNoReconnect()
    {
        Debug.WriteLine("OnTickerNoReconnect");
        ServerDisconnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnTickerReconnect()
    {
        Debug.WriteLine("OnTickerReconnect");
        ServerConnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnTick(Tick TickData)
    {
        Debug.WriteLine("OnTick" + TickData.ToString());
    }

    private void OnOrderUpdate(Order OrderData)
    {
        //IMPORTANT::This Places Reverse Order
        PlaceReverseOrder(OrderData);
    }

    private void PlaceReverseOrder(Order OrderData)
    {
        if (OrderData.Status != "COMPLETE")
        {
            return;
        }

        if (OrderData.TransactionType == KiteConnect.Constants.TRANSACTION_TYPE_BUY)
        {
            PlaceReverseSellOrder(OrderData);
            PlaceReverseBuyOrder(OrderData);
        }
        else
        {
            PlaceReverseBuyOrder(OrderData);
        }
    }

    private void PlaceReverseBuyOrder(Order OrderData)
    {
        var ltp = kite.GetLTP(new string[] { OrderData.InstrumentToken.ToString() })[OrderData.InstrumentToken.ToString()].LastPrice;
        decimal? factor = _selectedStocks.FirstOrDefault(item => item.Scrip == OrderData.Tradingsymbol)?.Factor;
        if (factor.HasValue)
        {
            factor = factor.Value;
        }
        else
        {
            factor = DefaultFactor;
        }

        try
        {
            Dictionary<string, dynamic> response = kite.PlaceOrder(
            Exchange: OrderData.Exchange,
            TradingSymbol: OrderData.Tradingsymbol,
            TransactionType: KiteConnect.Constants.TRANSACTION_TYPE_BUY,
            Quantity: OrderData.Quantity,
            Price: Math.Round(ltp / factor.Value, 1),
            OrderType: KiteConnect.Constants.ORDER_TYPE_LIMIT,
            Product: KiteConnect.Constants.PRODUCT_CNC,
            Validity: KiteConnect.Constants.VALIDITY_DAY
            );
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
        }
    }

    private void PlaceReverseSellOrder(Order OrderData)
    {
        var ltp = kite.GetLTP(new string[] { OrderData.InstrumentToken.ToString() })[OrderData.InstrumentToken.ToString()].LastPrice;
        var factor = _selectedStocks.FirstOrDefault(item => item.Scrip == OrderData.Tradingsymbol)?.Factor;
        if (factor.HasValue)
        {
            factor = factor.Value;
        }
        else
        {
            factor = DefaultFactor;
        }

        try
        {
            Dictionary<string, dynamic> response = kite.PlaceOrder(
            Exchange: OrderData.Exchange,
            TradingSymbol: OrderData.Tradingsymbol,
            TransactionType: KiteConnect.Constants.TRANSACTION_TYPE_SELL,
            Quantity: OrderData.Quantity,
            Price: Math.Round(ltp * factor.Value, 1),
            OrderType: KiteConnect.Constants.ORDER_TYPE_LIMIT,
            Product: KiteConnect.Constants.PRODUCT_CNC,
            Validity: KiteConnect.Constants.VALIDITY_DAY
            );
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
        }
    }

    internal void PlaceOpenOrder(ObservableCollection<StockItem> stocks)
    {
        _selectedStocks = stocks;

        foreach (var stock in stocks)
        {
            if (stock.Scrip == string.Empty || stock.Quantity == 0)
            {
                continue;
            }
            var price = kite.GetOHLC(new string[] { stock.InstrumentToken.ToString() })[stock.InstrumentToken.ToString()].Open;
            try
            {
                Dictionary<string, dynamic> response = kite.PlaceOrder(
                Exchange: KiteConnect.Constants.EXCHANGE_NSE,
                TradingSymbol: stock.Scrip,
                TransactionType: KiteConnect.Constants.TRANSACTION_TYPE_BUY,
                Quantity: stock.Quantity,
                Price: Math.Round(price / stock.Factor, 1),
                OrderType: KiteConnect.Constants.ORDER_TYPE_LIMIT,
                Product: KiteConnect.Constants.PRODUCT_CNC,
                Validity: KiteConnect.Constants.VALIDITY_DAY
                );
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }


    }

    public HistoricalModel GetHistoricalData(string tradingSymbol)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                var url = "https://groww.in/v1/api/charting_service/v2/chart/delayed/exchange/NSE/segment/CASH/" + tradingSymbol + "/daily?intervalInMinutes=1";
                string json = client.GetStringAsync(url).GetAwaiter().GetResult();
                HistoricalModel data = JsonSerializer.Deserialize<HistoricalModel>(json);
                return data;
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }
    }
}

public class HistoricalModel
{
    public decimal?[][] candles { get; set; }
}

public class HighLowModel
{
    public decimal high { get; set; }
    public decimal low { get; set; }
}