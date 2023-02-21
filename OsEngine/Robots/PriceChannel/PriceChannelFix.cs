using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OsEngine.Robots.PriceChannelCustom
{
    public class PriceChannelFix : BotPanel
    {
        #region Fields===========================================================================

        BotTabSimple tab;
        Aindicator pc;
        StrategyParameterInt lengthUp;
        StrategyParameterInt lengthDown;
        StrategyParameterString mode;
        StrategyParameterInt lot;
        StrategyParameterDecimal risk;

        #endregion

        #region Methods==========================================================================

        public PriceChannelFix(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple.First();

            lengthUp = CreateParameter("Length Channel Up", 12, 5, 80, 2);
            lengthDown = CreateParameter("Length Channel Down", 12, 5, 80, 2);
            mode = CreateParameter("Mode", "Off", new string[] { "Off", "On", "OnlyLong", "OnlyShort", "OnlyClosingPosition" });
            //lot = CreateParameter("Lot", 10, 5, 20, 1);
            risk = CreateParameter("Risk", 1m, 0.25m, 3m, 0.1m);

            pc = IndicatorsFactory.CreateIndicatorByName("PriceChannel", name + "PriceChannel", false);
            pc.ParametersDigit[0].Value = lengthUp.ValueInt;
            pc.ParametersDigit[1].Value = lengthDown.ValueInt;
            pc = (Aindicator)tab.CreateCandleIndicator(pc, "Prime");
            pc.Save();

            tab.CandleFinishedEvent += Tab_CandleFinishedEvent;
        }

        private void Tab_CandleFinishedEvent(List<Candle> candles)
        {
            if (!HasPermissionToOpenPosition())            
                return;

            if (pc.DataSeries[0] == null || pc.DataSeries[1] == null)
                return;

            if (pc.DataSeries[0].Values.Count < lengthUp.ValueInt + 1 || pc.DataSeries[1].Values.Count < lengthDown.ValueInt + 1)            
                return;

            var candle = candles[candles.Count - 1];
            var lastUp = pc.DataSeries[0].Values[pc.DataSeries[0].Values.Count - 2];
            var lastDown = pc.DataSeries[1].Values[pc.DataSeries[1].Values.Count - 2];

            var positions = tab.PositionsOpenAll;

            if (HasPermissionToOpenLongPosition() && candle.Close > lastUp && candle.Open < lastUp && positions.Count == 0)
            {                
                var lot = GetLots(lastUp, lastDown);
                tab.BuyAtMarket(lot);
                //tab.BuyAtMarket(lot.ValueInt);
            }
            else if(HasPermissionToOpenShortPosition() && candle.Close<lastDown && candle.Open>lastDown && positions.Count == 0)
            {
                var lot = GetLots(lastUp, lastDown);
                tab.SellAtMarket(lot);
                //tab.SellAtMarket(lot.ValueInt);
            }

            if (positions.Count > 0)            
                Trailing(positions);          
        }

        private bool HasPermissionToOpenPosition()
        {
            return mode.ValueString != "Off";
        }

        private bool HasPermissionToOpenLongPosition()
        {
            return mode.ValueString == "On" || mode.ValueString == "OnlyLong";
        }

        private bool HasPermissionToOpenShortPosition()
        {
            return mode.ValueString == "On" || mode.ValueString == "OnlyShort";
        }

        private int GetLots(decimal lastUp, decimal lastDown)
        {
            var riskMoney = tab.Portfolio.ValueBegin * risk.ValueDecimal / 100;
            //var costPriceStep = tab.Securiti.PriceStepCost; //для реальных торгов
            var costPriceStep = 1;                          //для лаборатории
            var steps = (lastUp - lastDown) / tab.Securiti.PriceStep;
            var lot = riskMoney / (steps * costPriceStep);

            return (int)lot;
        }

        private void Trailing(List<Position> positions)
        {
            foreach (var position in positions)
                if (position.State == PositionStateType.Open)
                    if (position.Direction == Side.Buy)
                    {
                        var lastDown = pc.DataSeries[1].Values.Last();
                        tab.CloseAtTrailingStop(position, lastDown, lastDown - 100 * tab.Securiti.PriceStep);
                    }
                    else if (position.Direction == Side.Sell)
                    {
                        var lastUp = pc.DataSeries[0].Values.Last();
                        tab.CloseAtTrailingStop(position, lastUp, lastUp + 100 * tab.Securiti.PriceStep);
                    }
        }

        public override string GetNameStrategyType()
        {
            return nameof(PriceChannelFix);
        }

        public override void ShowIndividualSettingsDialog()
        {
        }

        #endregion
    }
}