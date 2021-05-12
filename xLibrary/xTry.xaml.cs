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
    /// Логика взаимодействия для xTry.xaml
    /// </summary>
    public partial class xTry : UserControl
    {
        public xTry()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return txtBox.Text; }
            set { txtBox.Text = value;}
        }
        public string strWidth
        {
            get { return Convert.ToString(txtBox.ActualWidth); }
            set { }
        }

        private void Label_LayoutUpdated(object sender, EventArgs e)
        {
            Label.Text = Convert.ToString(txtBox.ActualWidth);
        }
    }
}
