using OkonkwoOandaV20;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.MyEntity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using TradesTSLAB.Commands;
using WPF_MVVM.ViewModels;

namespace OsEngine.ViewModels
{
    public class MyRobotVM:BaseVM
    {
        #region Properties ===========================================
        public DelegateCommand CommandServersToConnect
        {
            get
            {
                if (commandServersToConnect == null)
                {
                    commandServersToConnect = new DelegateCommand(ServersToConnect);
                }

                return commandServersToConnect;
            }
        }

        public ObservableCollection<string> ListSecurities { get; set; } = new ObservableCollection<string>();
        public string SelectedSecurity
        {
            get => selectedSecurity;
            set
            {
                selectedSecurity = value;
                OnPropertyChanged(nameof(SelectedSecurity));
                security = GetSecurityForName(selectedSecurity);
                StartSecurity(security);
            }
        }

        public decimal StartPoint
        {
            get => startPoint;
            set
            {
                startPoint = value;
                OnPropertyChanged(nameof(StartPoint));
            }
        }

        private decimal startPoint;

        public int CountLevels
        {
            get => countLevels;
            set
            {
                countLevels = value;
                OnPropertyChanged(nameof(CountLevels));
            }
        }

        private int countLevels;

        public Direction Direction
        {
            get => direction;
            set 
            {
                direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }

        private Direction direction;

        public List<Direction> Directions { get; set; } = new List<Direction>()
        {
            Direction.Buy, Direction.Sell, Direction.BuySell
        };

        public decimal Lot
        {
            get => lot;
            set
            {
                lot = value;
                OnPropertyChanged(nameof(Lot));
            }
        }

        private decimal lot;

        public StepType StepType
        {
            get => stepType;
            set
            {
                stepType = value;
                OnPropertyChanged(nameof(StepType));
            }
        }

        private StepType stepType;

        public List<StepType> StepTypes { get; set; } = new List<StepType>()
        { 
            StepType.Percent,
            StepType.Puncts
        };

        public decimal StepLevel
        {
            get => stepLevel;
            set
            {
                stepLevel = value;
                OnPropertyChanged(nameof(StepLevel));
            }
        }

        private decimal stepLevel;

        public decimal TakeLevel
        {
            get => takeLevel;
            set
            {
                takeLevel = value;
                OnPropertyChanged(nameof(TakeLevel));
            }
        }

        private decimal takeLevel;

        public int MaxActiveLevel
        {
            get => maxAciveLevel;
            set
            {
                maxAciveLevel = value;
                OnPropertyChanged(nameof(MaxActiveLevel));
            }
        }

        private int maxAciveLevel;

        public int AllPositionsCount
        {
            get => allPositionsCount;
            set
            {
                allPositionsCount= value;
                OnPropertyChanged(nameof(AllPositionsCount));
            }
        }

        private int allPositionsCount;

        public decimal PriceAverage
        {
            get => priceAverage;
            set
            {
                priceAverage = value;
                OnPropertyChanged(nameof(PriceAverage));
            }
        }

        private decimal priceAverage;

        public decimal VarMargine
        {
            get => varMargine;
            set
            {
                varMargine = value;
                OnPropertyChanged(nameof(VarMargine));
            }
        }

        private decimal varMargine;

        public decimal Accum
        {
            get => accum;
            set
            {
                accum = value;
                OnPropertyChanged(nameof(Accum));
            }
        }

        private decimal accum;

        public decimal Total
        {
            get => total;
            set
            {
                total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        private decimal total;


        #endregion

        #region Fields ==================================================================

        IServer server;
        List<Security> securities = new List<Security>();
        string selectedSecurity = string.Empty;
        Security security;

        #endregion

        #region Commands ================================================================

        private DelegateCommand commandServersToConnect;

        #endregion

        #region Methods ===========================================

        public MyRobotVM()
        {
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;

        }

        private void ServersToConnect(object o)
        {
            ServerMaster.ShowDialog(isTester: false);
        }

        private Security GetSecurityForName(string name)
        {
            for (int i = 0; i < securities.Count; i++)            
                if (securities[i].Name==name)
                    return securities[i];            
            return null;
        }

        private void StartSecurity(Security security)
        {
            if (security == null)
            {
                Debug.WriteLine("StartSecurity security == null");
                return;
            }

            Task.Run(() =>
            {
                while (true)
                {
                    var series = server.StartThisSecurity(security.Name, new TimeFrameBuilder(), security.NameClass);
                    if (series != null)
                        break;
                }

                Thread.Sleep(1000);
            });
        }

        private void ServerMaster_ServerCreateEvent(IServer newServer)
        {
            if (newServer == server)
                return;

            server = newServer;

            server.PortfoliosChangeEvent += NewServer_PortfoliosChangeEvent;
            server.SecuritiesChangeEvent += NewServer_SecuritiesChangeEvent;
            server.NeadToReconnectEvent += NewServer_NeadToReconnectEvent;
            server.NewMarketDepthEvent += NewServer_NewMarketDepthEvent;
            server.NewTradeEvent += NewServer_NewTradeEvent;
            server.NewOrderIncomeEvent += NewServer_NewOrderIncomeEvent;
            server.NewMyTradeEvent += NewServer_NewMyTradeEvent;
            server.ConnectStatusChangeEvent += NewServer_ConnectStatusChangeEvent;
        }

        private void NewServer_ConnectStatusChangeEvent(string obj)
        {            
        }

        private void NewServer_SecuritiesChangeEvent(List<Security> securities)
        {
            ObservableCollection<string> listSecurities = new ObservableCollection<string>();
            for (int i = 0; i < securities.Count; i++)
            {
                listSecurities.Add(securities[i].Name);
            }

            ListSecurities = listSecurities;
            OnPropertyChanged(nameof(ListSecurities));
            this.securities = securities;
        }

        private void NewServer_PortfoliosChangeEvent(List<Portfolio> portfolios)
        {
        }

        private void NewServer_NewMyTradeEvent(MyTrade myTrade)
        {
        }

        private void NewServer_NewOrderIncomeEvent(Order order)
        {
        }

        private void NewServer_NewTradeEvent(List<Trade> trades)
        {
        }

        private void NewServer_NewMarketDepthEvent(MarketDepth marketDepth)
        {
        }

        private void NewServer_NeadToReconnectEvent()
        {            
        }

        #endregion
    }
}