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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace xLibrary
{
    


    /// <summary>
    /// Логика взаимодействия для xOnOff.xaml
    /// </summary>
    public partial class xOnOff : UserControl
    {
        private bool _state = false;
        public bool State
        {
            get { return _state; }
            set
            {
                _state = value;
                AnimateSwitch(_state);
            }
        }
        public event RoutedEventHandler SwitchChanged;
        
        private void AnimateSwitch(bool on)
        {
            double offset = back.ActualWidth / 4 - this.ActualWidth * 0.05;
            double coeff = -1;
            TranslateTransform trans = new TranslateTransform();
            swtch.RenderTransform = trans;
            if (on) coeff = 1; else coeff = -1;
            DoubleAnimation anim = new DoubleAnimation(-coeff*offset, coeff*offset, TimeSpan.FromMilliseconds(250));
            trans.BeginAnimation(TranslateTransform.XProperty, anim);

            if (SwitchChanged != null) SwitchChanged(this, new RoutedEventArgs());
        }
        public xOnOff()
        {
            InitializeComponent();
            _buildSwitcher();
        }

        private void _buildSwitcher()
        {
            swtch.Width = this.ActualWidth * 0.5;
            swtch.Height = this.ActualHeight * 0.8;

            double offset = - ( back.ActualWidth / 4 - this.ActualWidth * 0.05);
            var T = new TranslateTransform(offset, 0);
            swtch.RenderTransform = T;
        }
        
        private void swtch_Loaded(object sender, RoutedEventArgs e)
        {
            _buildSwitcher();
        }
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            State = !State;
        }



        
    }
}
