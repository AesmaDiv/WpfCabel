using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Dot : UserControl
    {
        public Dot()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center",
            typeof(Point),
            typeof(Dot),
            new PropertyMetadata(new Point(), OnCenterChanged));


        public Point Center
        {
            set { SetValue(CenterProperty, value); }
            get { return (Point)GetValue(CenterProperty); }
        }
        public Brush Fill
        {
            set { path.Fill = value; }
            get { return this.Fill; }
        }

        static void OnCenterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Dot).ellipseGeo.Center = (Point)args.NewValue;
        }
    }
}
