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
    public partial class CrossMark : UserControl
    {
        public CrossMark()
        {
            InitializeComponent();
        }
        public CrossMark(string name)
        {
            InitializeComponent();
            this.Name = name;
        }
        public CrossMark(string name, Brush color)
        {
            InitializeComponent();
            this.Name = name;
            Fill = color;
        }
        
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center",
            typeof(Point),
            typeof(CrossMark),
            new PropertyMetadata(new Point(), OnCenterChanged));


        public Point Center
        {
            set { SetValue(CenterProperty, value); }
            get { return (Point)GetValue(CenterProperty); }
        }
        public Brush Fill
        {
            set { path.Fill = value; }
            get { return path.Fill; }
        }

        static void OnCenterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Canvas.SetLeft((obj as CrossMark),((Point)args.NewValue).X );
            Canvas.SetTop((obj as CrossMark), ((Point)args.NewValue).Y);
        }
    }
}

