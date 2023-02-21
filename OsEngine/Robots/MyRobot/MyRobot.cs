using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OsEngine.Robots
{
    public class MyRobot : BotPanel
    {
        #region Properties================================================================

        public string[] Modes { get; } = new string[3] {Strategy.None.ToString(), Strategy.TrailingStop.ToString(), Strategy.TwoTakes.ToString() };
        public string Mode { get => mode.ValueString; set => mode.ValueString = value; }
        public int Lot { get; set; }
        public int Stop { get; set; }
        public int Take { get; set; }
        public decimal TakeStopKoef { get => _profitKoef.ValueDecimal; set => _profitKoef.ValueDecimal = value; }
        private Strategy Strategy { get => GetStrategy(); }


        #endregion

        #region Fields===================================================================

        decimal entryPrice;
        Candle lastCandle;
        List<Candle> candles;
        BotTabSimple _tab;
        StrategyParameterString mode;

        /// <summary>
        /// Риск на сделку
        /// </summary>
        StrategyParameterDecimal _risk;

        /// <summary>
        /// Во сколько раз тейк больше риска
        /// </summary>
        StrategyParameterDecimal _profitKoef;

        /// <summary>
        /// Кол-во падающих свечей перед объёмным разворотом
        /// </summary>
        StrategyParameterInt _countDownCandles;

        /// <summary>
        /// Во сколько раз объём превышает средний
        /// </summary>
        StrategyParameterDecimal _koefVolume;

        /// <summary>
        /// Средний объём
        /// </summary>
        decimal _averageVolume;

        /// <summary>
        /// Количество свечей для вычисления среднего объёма
        /// </summary>
        StrategyParameterInt _countCandles;

        /// <summary>
        /// Количество пунктов  до стоп-лосса
        /// </summary>
        int _punkts = 0;
        decimal stopPrice = 0m;

        #endregion

        #region Methods===================================================================

        public MyRobot(string name, StartProgram startProgram) : base(name, startProgram)
        {            
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            mode = CreateParameter(nameof(Mode), Modes[0], Modes);
            _risk = CreateParameter("Risk %", 1m, 0.1m, 10m, 0.1m);
            _profitKoef = CreateParameter("Koef Profit %", 1m, 0.1m, 10m, 0.1m);
            _countDownCandles = CreateParameter("Count down candles", 2, 1, 5, 1);
            _koefVolume = CreateParameter("Koef volume", 2m, 2m, 10m, 0.5m);
            _countCandles = CreateParameter("Count candles", 10, 5, 50, 1);

            _tab.CandleFinishedEvent += _tab1_CandleFinishedEvent;
            _tab.PositionOpeningSuccesEvent += _tab_PositionOpeningSuccesEvent;
            _tab.PositionClosingSuccesEvent += _tab_PositionClosingSuccesEvent;
        }

        private void _tab1_CandleFinishedEvent(List<Candle> candles)
        {
            if (candles.Count < _countDownCandles.ValueInt + 1 || candles.Count < _countCandles.ValueInt + 1)
                return;

            this.candles = candles;
            lastCandle = candles[candles.Count - 1];

            _averageVolume = 0;
            for (int i = candles.Count - 2; i > candles.Count - _countCandles.ValueInt - 2; i--)            
                _averageVolume+= candles[i].Volume;
            _averageVolume /= _countCandles.ValueInt;

            List<Position> positions = _tab.PositionOpenLong;
            if (positions.Count == 0)
            {
                if (WasGotSignal())
                {
                    OpenPosition("LE1");
                    if (Strategy == Strategy.TwoTakes)
                        OpenPosition("LE2");
                }                
            }            
            else 
            {
                var position = _tab.PositionOpenLong.First();
                UpdateStopLoss(position);                
            }
        }

        private bool WasGotSignal()
        {

            if (lastCandle.Close < (lastCandle.High + lastCandle.Low) / 2 || lastCandle.Volume < _averageVolume * _koefVolume.ValueDecimal)
                return false;

            for (int i = candles.Count - 2; i > candles.Count - 2 - _countDownCandles.ValueInt; i--)
                if (candles[i].Close > candles[i].Open)
                    return false;

            _punkts = (int)((lastCandle.Close - lastCandle.Low) / _tab.Securiti.PriceStep);
            if (_punkts < 5)
                return false;            

            return true;
        }

        private void OpenPosition(string signalType)
        {
            decimal amountStop = _punkts * _tab.Securiti.PriceStepCost;
            decimal amountRisk = _tab.Portfolio.ValueBegin * _risk.ValueDecimal / 100;
            decimal volume = amountRisk / amountStop;

            decimal go = 10000;

            if (_tab.Securiti.Go > 1)
                go = _tab.Securiti.Go;

            decimal maxLot = _tab.Portfolio.ValueBegin / go;
            if (volume < maxLot)
            {
                stopPrice = lastCandle.Low;
                _tab.BuyAtMarket(volume, signalType);
            }
        }

        private void UpdateStopLoss(Position position)
        {
            if (Strategy == Strategy.TrailingStop)            
                if (lastCandle.High - entryPrice >= entryPrice - stopPrice)
                    _tab.CloseAtStop(position, entryPrice, entryPrice - 100 * _tab.Securiti.PriceStep);            
        }

        private Strategy GetStrategy()
        {
            var strategy = mode.ValueString;
            strategy = strategy.ToLower();              //этот приём позволяет сократить количество ошибок
            if (strategy.Contains("trailingstop"))
                return Strategy.TrailingStop;
            if (strategy.Contains("twotakes"))
                return Strategy.TwoTakes;
            return Strategy.None;
        }        

        private void _tab_PositionOpeningSuccesEvent(Position pos)
        {
            entryPrice = pos.EntryPrice;            
            _tab.CloseAtStop(pos, stopPrice, stopPrice - 100 * _tab.Securiti.PriceStep);

            var priceTake = 0m;

            if (pos.SignalTypeOpen == "LE1")            
                priceTake = pos.EntryPrice + _punkts * _profitKoef.ValueDecimal;            
            else if (pos.SignalTypeOpen == "LE2")            
                priceTake = pos.EntryPrice + _punkts;            
            
            _tab.CloseAtProfit(pos, priceTake, priceTake);            
        }

        private void _tab_PositionClosingSuccesEvent(Position pos)
        {
            SaveCSV(pos);
        }

        private void SaveCSV(Position pos)
        {
            if (!File.Exists(@"Engine\trades.csv"))
            {
                string header = "; Позиция; Символ; Лоты; Изменение / Максимум Лотов; Исполнение входа; Сигнал входа; Бар входа; Дата входа; Время входа; Цена входа; Комиссия входа; " +
               "Исполнение выхода; Сигнал выхода; Бар выхода; Дата выхода; Время выхода; Цена выхода; Комиссия выхода; Средневзвешенная цена входа; П / У; П / У сделки; " +
               "П / У с одного лота; Зафиксированная П/ У; Открытая П/ У; Продолж. (баров); Доход / Бар; Общий П/ У;% изменения; MAE; MAE %; MFE; MFE %";

                using (StreamWriter writer = new StreamWriter(@"Engine\trades.csv", false, Encoding.UTF8))
                {
                    writer.WriteLine(header);
                    writer.Close();
                }
            }

            using (StreamWriter writer = new StreamWriter(@"Engine\trades.csv", true, Encoding.UTF8))
            {
                string str = ";;;;;;;;" + pos.TimeOpen.ToShortDateString();
                str += ";" + pos.TimeOpen.TimeOfDay;
                str += ";;;;;;;;;;;;;;" + pos.ProfitPortfolioPunkt + ";;;;;;;;;";

                writer.WriteLine(str);
                writer.Close();
            }
        }

        public override string GetNameStrategyType()
        {
            return nameof(MyRobot);
        }

        public override void ShowIndividualSettingsDialog()
        {
            WindowMyRobot window = new WindowMyRobot(this);
            window.ShowDialog();
        }

        public void UpdateParameter(string parameter)
        {
            if (parameter == nameof(Mode))
                mode.ValueString = Mode;
            if (parameter == nameof(TakeStopKoef))
                _profitKoef.ValueDecimal = TakeStopKoef;
        }

        #endregion
    }
}