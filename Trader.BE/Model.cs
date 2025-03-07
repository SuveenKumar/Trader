using KiteConnect;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel;
using System.Runtime.CompilerServices;

    public class StockItem : INotifyPropertyChanged
    {
        private int _serialNo;
        private string _scrip;
        private int _quantity;
        private decimal _factor;
        private List<Instrument> instruments;
        private Kite kite;
        public string InstrumentToken;
        public StockItem(Kite aKite, List<Instrument> aInstruments)
        {
            kite = aKite;
            instruments = aInstruments;
        }
        public int SerialNo
        {
            get => _serialNo;
            set { _serialNo = value; OnPropertyChanged(); }
        }

        public string Scrip
        {
            get => _scrip;
            set
            {
                _scrip = value;
                if (_scrip != "")
                {
                    foreach (var inst in instruments)
                    {
                        if (inst.TradingSymbol == _scrip)
                        {
                            InstrumentToken = inst.InstrumentToken.ToString();
                            break;
                        }
                    }
                    var ltp = kite.GetLTP(new string[] { InstrumentToken.ToString() })[InstrumentToken.ToString()].LastPrice;
                    Quantity = (int)(5000 / ltp);
                }
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public decimal Factor
        {
            get => _factor;
            set { _factor = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

public class StockHub : Hub
{
    public StockHub()
    {
        Console.WriteLine();
    }
    public async Task SendStockUpdate(string scrip, int quantity, decimal factor, decimal price)
    {
        await Clients.All.SendAsync("ReceiveStockUpdate", scrip, quantity, factor, price);
    }
}
