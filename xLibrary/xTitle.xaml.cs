using System;
using System.Collections.Generic;
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
    /// Interaction logic for xTitle.xaml
    /// </summary>
    public partial class xTitle : UserControl
    {
        public event MouseButtonEventHandler title_MouseDown;
        public event MouseEventHandler Close_Click;
        public event MouseEventHandler Max_Click;
        public event MouseEventHandler Min_Click;

        public Brush MenuBackground
        {
            get { return expMenu.Background; }
            set { expMenu.Background = value; }
        }

        public string Header
        {
            get { return txtTitle.Content.ToString(); }
            set { txtTitle.Content = value; }
        }
        public xTitle()
        {
            this.InitializeComponent();
            //Button btn = new Button();
            //btn.Width = 150;
            //btn.Height = 30;
            //bdrContent.Child=btn;
        }
        public void SetContent(UIElement content)
        {
            frmContent.Content = content;
        }
        public void HideMenu()
        {
            expMenu.IsExpanded = false;
        }

        private void bdrWindowTitle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (title_MouseDown != null)
                title_MouseDown(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
            if ((e.ClickCount > 1) && (Max_Click != null))
                Max_Click(this, new MouseEventArgs(Mouse.PrimaryDevice, 0, Stylus.CurrentStylusDevice));
        }
        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Close_Click != null)
                Close_Click(this, new MouseEventArgs(Mouse.PrimaryDevice, 0, Stylus.CurrentStylusDevice));
        }
        private void btnMax_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Max_Click != null)
                Max_Click(this, new MouseEventArgs(Mouse.PrimaryDevice, 0, Stylus.CurrentStylusDevice));
        }
        private void btnMin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Min_Click != null)
                Min_Click(this, new MouseEventArgs(Mouse.PrimaryDevice, 0, Stylus.CurrentStylusDevice));
        }        
    }
}