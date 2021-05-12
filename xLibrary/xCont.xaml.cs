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
using System.ComponentModel;
using System.Media;
using System.Windows.Media.Animation;

namespace xLibrary
{
    /// <summary>
    /// Логика взаимодействия для xCont.xaml
    /// </summary>
    public partial class xCont : UserControl
    {       
        private string _direction = "Right";
        public string Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
            }
        }       
        double expanded_width;
        int count = 0;
        private string _header = "myExpander";
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }
        bool _isFixed = false;
        public bool IsFixed
        {
            get { return _isFixed; }
            set
            {
                _isFixed = value;
                SetPin();
            }
        }
        public event RoutedEventHandler Expanded_Changed;
        bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; }
        }

        public xCont()
        {
            InitializeComponent();
        }

        private void Pin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsFixed = !IsFixed;
        }
        private void SetPin()
        {
            Border pin = this.Template.FindName("Pin", this) as Border;
            if (pin != null) if (IsFixed) pin.Background = Brushes.Green;
            else pin.Background = Brushes.Transparent;
        }

        private void xExpander_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.Content == null) return;
            if (!IsFixed) StartAnimation(this.ActualWidth, expanded_width);
            _isExpanded = !_isExpanded;
        }
        private void xExpander_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.Content == null) return;
            if (!IsFixed) StartAnimation(this.ActualWidth, 20);
        }
        private void StartAnimation(double start_size, double stop_size)
        {
            DoubleAnimation da = new DoubleAnimation(start_size, stop_size, TimeSpan.FromSeconds(0.25));
            Storyboard sb = new Storyboard();
            Storyboard.SetTarget(da, this);
            Storyboard.SetTargetProperty(da, new PropertyPath(WidthProperty));
            sb.Completed += sb_Completed;
            sb.Children.Add(da);
            sb.BeginTime = TimeSpan.FromSeconds(0.25);
            sb.Begin();
            
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            _isExpanded = !_isExpanded;
            if (Expanded_Changed != null) Expanded_Changed(this, new RoutedEventArgs());
        }
        private void xExpander_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (count < 2)
            {
                ContentPresenter cp = this.Template.FindName("contentPresenter", this) as ContentPresenter;
                expanded_width = this.ActualWidth;
                count++;
                if ((count == 2) && (!_isFixed)) this.Width = 20;
            }
        }
        public void SetContent(object content)
        {
            this.Content = content;
            this.Width = Double.NaN;
            //count = 0;
        }

        private void xExpander_Loaded(object sender, EventArgs e)
        {
            Label label = this.Template.FindName("Label", this) as Label;
            ContentPresenter content = this.Template.FindName("contentPresenter", this) as ContentPresenter;
            Border pin = this.Template.FindName("Pin", this) as Border;
            if (_direction == "Right")
            {
                DockPanel.SetDock(label, Dock.Right);
                DockPanel.SetDock(content, Dock.Left);
                pin.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                DockPanel.SetDock(label, Dock.Left);
                DockPanel.SetDock(content, Dock.Right);
                pin.HorizontalAlignment = HorizontalAlignment.Left;
            }
            SetPin();
        }
        public void Expanded(bool state)
        {
            if (state) xExpander_MouseEnter(this, new MouseEventArgs(Mouse.PrimaryDevice, 0));
            else xExpander_MouseLeave(this, new MouseEventArgs(Mouse.PrimaryDevice, 0));
        }           
        public void SetWidth(double new_width)
        {
            expanded_width = new_width;
        }
    }
}
