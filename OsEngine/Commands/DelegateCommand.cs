using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TradesTSLAB.Commands
{
    public class DelegateCommand : ICommand
    {
        public delegate void DelegateFunction(object obj);
        public event EventHandler CanExecuteChanged;
        private DelegateFunction function;

        public DelegateCommand(DelegateFunction function)
        {
            this.function = function;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            function?.Invoke(parameter);
        }
    }
}