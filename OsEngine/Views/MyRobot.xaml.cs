using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace OsEngine.Views
{
    /// <summary>
    /// Логика взаимодействия для MyRobot.xaml
    /// </summary>
    public partial class MyRobot : MetroWindow
    {
        MyRobotVM vm;

        public MyRobot()
        {
            InitializeComponent();
            vm = new MyRobotVM();
            DataContext = vm;
        }
    }
}
