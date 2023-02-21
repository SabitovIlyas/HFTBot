using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OsEngine.Robots.CryptoCluster
{
    public class RobotCluster : BotPanel
    {
        #region Fileds========================================================================
        
        private BotTabSimple tabSimple;
        private BotTabCluster tabCluster;
        public StrategyParameterBool Mode;
        public StrategyParameterInt Koef;
        public StrategyParameterInt CountCandles;
        public StrategyParameterDecimal Risk;
        public StrategyParameterInt Stop;
        public StrategyParameterInt Take;
        public StrategyParameterInt Depo;
        public StrategyParameterInt MinVolumeDollar;
        private Aindicator atr;
        private decimal stopPrice;
        private decimal takePrice;


        #endregion

        #region Methods=======================================================================

        public RobotCluster(string name, StartProgram startProgram): base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tabSimple = TabsSimple.First();
            TabCreate(BotTabType.Cluster);
            tabCluster = TabsCluster.First();
            
            Mode = CreateParameter("Mode", false);
            Koef = CreateParameter("koef", 3, 3, 9, 1);
            CountCandles = CreateParameter("CountCandles", 5, 3, 9, 1);
            Risk = CreateParameter("Risk", 1m, 0.1m, 2m, 0.1m);
            Stop = CreateParameter("Stop ATR", 1, 1, 1, 1);
            Take = CreateParameter("Take ATR", 3, 1, 10, 1);
            Depo = CreateParameter("Depo", 1000, 1000, 1000, 1);
            MinVolumeDollar = CreateParameter("MinVolumeDollar", 100000, 100000, 100000, 50000);

            atr = IndicatorsFactory.CreateIndicatorByName("ATR", name + "ATR", false);
            atr.ParametersDigit[0].Value = 100;
            atr = (Aindicator)tabSimple.CreateCandleIndicator(atr, "Second");
            atr.PaintOn = true;
            atr.Save();

            tabSimple.CandleFinishedEvent += tabSimple_CandleFinishedEvent;            
        }
        
        private void tabSimple_CandleFinishedEvent(List<Candle> candles)
        {
            if (candles.Count < CountCandles.ValueInt || tabCluster.VolumeClusters.Count < CountCandles.ValueInt)            
                return;            

            var positions = tabSimple.PositionOpenLong;
            if (positions == null || positions.Count == 0)
            {
                var average = 0m;
                for (var i = tabCluster.VolumeClusters.Count - CountCandles.ValueInt - 1; i < tabCluster.VolumeClusters.Count - 2; i++)
                {
                    average += tabCluster.VolumeClusters[i].MaxSummVolumeLine.VolumeSumm;

                }

                average /= (CountCandles.ValueInt - 1);

                var last = tabCluster.VolumeClusters[tabCluster.VolumeClusters.Count - 2].MaxSummVolumeLine;

                if (last.VolumeSumm > average * Koef.ValueInt && last.VolumeDelta < 0 && last.VolumeSumm * last.Price > MinVolumeDollar.ValueInt)
                {
                    var lastAtr = atr.DataSeries[0].Last;
                    var moneyRisk = Depo.ValueInt * Risk.ValueDecimal / 100;
                    var volume = moneyRisk / (lastAtr * Stop.ValueInt);

                    tabSimple.BuyAtMarket(volume);
                    stopPrice = candles[candles.Count - 1].Close - lastAtr;
                    takePrice = candles[candles.Count - 1].Close + lastAtr * Take.ValueInt;
                }
                else
                {
                    foreach (var position in positions)
                    {
                        if (position.State == PositionStateType.Open)
                        {
                            tabSimple.CloseAtStop(position, stopPrice, stopPrice - 100 * tabSimple.Securiti.PriceStep);
                            tabSimple.CloseAtProfit(position, takePrice, takePrice);

                        }
                    }
                }
            }            
        }

        public override string GetNameStrategyType()
        {
            return nameof(RobotCluster);
        }

        public override void ShowIndividualSettingsDialog()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
