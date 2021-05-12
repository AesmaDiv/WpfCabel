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
    /// Логика взаимодействия для xAnalog.xaml
    /// </summary>
    public partial class xAnalog : UserControl
    {
        public double Value;

        Pen pen = new Pen();
        
        public xAnalog()
        {
            InitializeComponent();
            pen.Brush = Brushes.White;
            pen.Thickness = 2;
            
            
        }
        private void DrawAnalog()
        {

        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            
            canvas.Background = Brushes.Green;
            Line line = new Line();
            line.Stroke = Brushes.Black;
            line.X1 = ActualWidth / 2;
            line.Y1 = ActualHeight;
            line.X2 = 10;
            line.Y2 = 10;
            canvas.Children.Add(line);
        }
    }
}
