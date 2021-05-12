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
    /// Логика взаимодействия для xSwitch.xaml
    /// </summary>
    public partial class xSwitchOnOff : UserControl
    {
        public Orientation orient = Orientation.Vertical;
      
        public xSwitchOnOff()
        {
            InitializeComponent();
        }

        private void Switch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(Switch.VerticalAlignment == VerticalAlignment.Top)
                Switch.VerticalAlignment = VerticalAlignment.Bottom;
            else
                Switch.VerticalAlignment = VerticalAlignment.Top;
        }

        public void CreateSwitcher(int count)
        {
            for (int i = 0; i < count; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Height = 20;
                tb.Text = (i * 100).ToString();
                tb.Margin = new Thickness(5);
                stack.Children.Add(tb);

                Border bdr = new Border();
                bdr.Background = Brushes.Black;
                bdr.Width = 20;
                bdr.Height = 30*count - 10;
                if (i == 0) bdr.CornerRadius = new CornerRadius(10, 10, 0, 0);
                if (i == (count-2)) bdr.CornerRadius = new CornerRadius(0, 0, 10, 10);
                stkBack.Children.Add(bdr);
            }
            this.Height = 30 * count;
            this.Width = 140;
        }

    }
}
