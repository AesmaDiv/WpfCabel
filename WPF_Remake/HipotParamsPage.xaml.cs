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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_Try
{
    /// <summary>
    /// Логика взаимодействия для HipotParamsPage.xaml
    /// </summary>
    public partial class HipotParamsPage : Page
    {
        private string[] _voltages = new string[] { "12 кВ", "18 кВ" };
        private int[] _kilovolts = new int[] { 12, 18 };

        public int OutputVoltage
        {
            get
            {
                if (comboVoltage.SelectedItem == null) return 0;
                else return _kilovolts[comboVoltage.SelectedIndex];
            }
        }
        public bool IsWithPolar
        { get { return checkPolar.IsChecked == true; } }

        public HipotParamsPage()
        {
            InitializeComponent();
            comboVoltage.ItemsSource = _voltages;
            checkPolar.IsChecked = false;
        }
    }
}
