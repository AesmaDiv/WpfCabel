using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace xLibrary
{
    /// <summary>
    /// Логика взаимодействия для xStatusIndicator.xaml
    /// </summary>
    public partial class xStatusIndicator : UserControl
    {
        public static DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(bool), typeof(xStatusIndicator), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public bool Status
        {
            get { return (bool)this.GetValue(StatusProperty); }
            set { this.SetValue(StatusProperty, value); /*_SetStatus();*/ }
        }
        public string Text
        {
            get { return xText.Content.ToString(); }
            set { xText.Content = value; }
        }
        public xStatusIndicator()
        {
            InitializeComponent();
        }
        private void _SetStatus()
        {
            bool state = (bool)this.GetValue(StatusProperty);
            if (state) xImage.Background = (Brush)this.FindResource("status_on");
            else xImage.Background = (Brush)this.FindResource("status_off");
        }
    }
}
