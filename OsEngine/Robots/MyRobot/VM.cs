using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_MVVM.ViewModels;

namespace OsEngine.Robots
{
    public class VM : BaseVM
    {
        public string[] Modes
        {
            get => _robot.Modes;            
        }

        public string Mode
        {
            get => _robot.Mode;
            set
            {
                _robot.Mode = value;
                var parameter = nameof(Mode);
                OnPropertyChanged(parameter);
                _robot.UpdateParameter(parameter);
            }
        }

        public int Lot
        {
            get => _robot.Lot;
            set
            {
                _robot.Lot = value;
                var parameter = nameof(Lot);
                OnPropertyChanged(parameter);
            }
        }
        public int Stop
        {
            get => _robot.Stop;
            set
            {
                _robot.Stop = value;
                var parameter = (nameof(Stop));
                OnPropertyChanged(parameter);
            }
        }
        public int Take
        {
            get => _robot.Take;
            set
            {
                _robot.Take = value;
                var parameter = (nameof(Take));
                OnPropertyChanged(parameter);
            }
        }

        public decimal TakeStopKoef
        {
            get => _robot.TakeStopKoef;
            set
            {
                _robot.TakeStopKoef = value;
                var parameter = (nameof(TakeStopKoef));
                OnPropertyChanged(parameter);
                _robot.UpdateParameter(parameter);
            }
        }

        private MyRobot _robot;

        public VM(MyRobot robot)
        {
            _robot = robot;
        }
    }
}