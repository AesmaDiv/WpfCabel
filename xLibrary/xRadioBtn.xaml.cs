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
    /// Логика взаимодействия для xRadioBtn.xaml
    /// </summary>
    public partial class xRadioBtn : UserControl
    {
        private bool _is_checked = false;
        private bool _is_mouse_can_check = false;
        public bool IsChecked
        {
            get { return _is_checked; }
            set 
            { 
                _is_checked = value;
                SwitchState();
            }
        }
        public bool IsMouseCanSwitch
        {
            get { return _is_mouse_can_check; }
            set { _is_mouse_can_check = value; }
        }
        public double ButtonSize
        {
            get { return btn.Width; }
            set { btn.Width = value; }
        }
        public HorizontalAlignment HeaderAlignment
        {
            get { return header.HorizontalContentAlignment; }
            set
            {
                if (value == HorizontalAlignment.Left) DockPanel.SetDock(btn, Dock.Right);
                else DockPanel.SetDock(btn, Dock.Left);
                header.HorizontalContentAlignment = value;
            }
        }
        public string Header
        {
            get { return header.Content.ToString(); }
            set { header.Content = value; }
        }
        public event RoutedEventHandler OnClickEvent;
     
        public xRadioBtn()
        {
            InitializeComponent();
        }
        private void SwitchState()
        {
            btn.Background = _is_checked ? (RadialGradientBrush)this.FindResource("on") :
                                           (RadialGradientBrush)this.FindResource("off");
        }
        private void BroadcastEvent()
        {
            if (OnClickEvent != null) OnClickEvent(this, new RoutedEventArgs());
        }
        private void userControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_is_mouse_can_check) IsChecked = !IsChecked;
            BroadcastEvent();
        }
    }
}
