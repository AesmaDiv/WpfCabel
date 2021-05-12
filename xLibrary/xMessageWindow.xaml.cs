using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace xLibrary
{
    /// <summary>
    /// Логика взаимодействия для wndMessage.xaml
    /// </summary>
    public partial class xMessageWindow : Window
    {
        private MessageBoxResult _result = MessageBoxResult.OK;

        public string Title
        {
            get { return wndTitle.Text; }
            set { wndTitle.Text = value; }
        }
        public UIElement Content
        {
            get { return bdrContainer.Child; }
            set { bdrContainer.Child = value; }
        }
        public bool OkCancel
        { set { bdrCancel.Visibility = value ? Visibility.Visible : Visibility.Collapsed; } }
        
        public xMessageWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
        }
        public xMessageWindow(string title, UIElement content, bool ok_cancel)
        {
            InitializeComponent();
            Title = title;
            Content = content;
            OkCancel = ok_cancel;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public MessageBoxResult Show()
        {
            this.ShowDialog();
            return _result;
        }

        private void bdrCancel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _result = MessageBoxResult.No;
            this.Close();
        }
        private void bdrOK_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _result = MessageBoxResult.Yes;
            this.Close();
        }

        private void bdr_MouseEnter(object sender, MouseEventArgs e)
        {
            Border bdr = sender as Border;
            bdr.BorderBrush = Brushes.White;
        }
        private void bdr_MouseLeave(object sender, MouseEventArgs e)
        {
            Border bdr = sender as Border;
            bdr.BorderBrush = (Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString("#FF82BFC9");
        }
    }
}
