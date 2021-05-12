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

namespace xLibrary
{
    /// <summary>
    /// Логика взаимодействия для xProgressBar.xaml
    /// </summary>
    public partial class xProgressBar : UserControl
    {
        public double Value
        {
            get { return pb.Value;}
            set { pb.Value = value; }
        }
        public double MaxValue
        {
            get { return pb.Maximum; }
            set { pb.Maximum = value; }
        }
        public double MinValue
        {
            get { return pb.Minimum; }
            set { pb.Minimum = value; }
        }
        public string StringValue
        {
            get { return txtValue.Text; }
            set {  txtValue.Text = value; }
        }
        
        
        
        public xProgressBar()
        {
            InitializeComponent();
        }
    }
}
