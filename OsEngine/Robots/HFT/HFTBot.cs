using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.OsTrader.Panels;
using System.Collections.Generic;
using System.Threading.Tasks;
using OsEngine.Market.Servers;
using System.Diagnostics;
using System.Threading;

namespace OsEngine.Robots.HFT
{
    public class HFTBot : BotPanel
    {
        #region Fields =====================================================================
        
        List<IServer> servers = new List<IServer>();
        List<Portfolio> portfolios = new List<Portfolio>();
        string nameSecurity = "BTCUSDT";
        ServerType serverType = ServerType.Binance;
        Security security = null;
        IServer server;
        CandleSeries series = null;
        
        #endregion

        #region Methods ====================================================================
        
        public HFTBot(string name, StartProgram startProgram) : base(name, startProgram)
        {
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;
        }

        private void ServerMaster_ServerCreateEvent(IServer newServer)
        {            
            foreach(IServer server in servers)             
                if (newServer == server)
                    return;            

            if(newServer.ServerType == serverType)            
                server = newServer;            

            servers.Add(newServer);

            newServer.PortfoliosChangeEvent += NewServer_PortfoliosChangeEvent;
            newServer.SecuritiesChangeEvent += NewServer_SecuritiesChangeEvent;
            newServer.NeadToReconnectEvent += NewServer_NeadToReconnectEvent;
            newServer.NewMarketDepthEvent += NewServer_NewMarketDepthEvent;
            newServer.NewTradeEvent += NewServer_NewTradeEvent;
            newServer.NewOrderIncomeEvent += NewServer_NewOrderIncomeEvent;
            newServer.NewMyTradeEvent += NewServer_NewMyTradeEvent;
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
            StartSecurity(security);
        }

        private void NewServer_SecuritiesChangeEvent(List<Security> securities)
        {
            if (securities != null) return;

            for (int i = 0; i < securities.Count; i++)            
                if (nameSecurity == securities[i].Name)
                {
                    security = securities[i];
                    StartSecurity(security);
                    break;
                }           
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
                while(true)
                {
                    series = server.StartThisSecurity(security.Name, new TimeFrameBuilder(), security.NameClass);
                    if (series != null)
                        break;
                }

                Thread.Sleep(1000);
            });
        }

        private void NewServer_PortfoliosChangeEvent(List<Portfolio> newPortfolios)
        {
            for (int x = 0; x < newPortfolios.Count; x++)
            {
                var flag = true;

                for (int i = 0; i < portfolios.Count; i++)                
                    if (newPortfolios[x].Number == portfolios[i].Number)
                    {
                        flag = false;
                        break;
                    }                

                if (flag)                
                    portfolios.Add(newPortfolios[x]);               
            }
        }

        public override string GetNameStrategyType()
        {
            return nameof(HFTBot);
        }

        public override void ShowIndividualSettingsDialog()
        {
        }

        #endregion
    }
}