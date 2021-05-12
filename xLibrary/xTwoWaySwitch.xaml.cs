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
    /// Логика взаимодействия для xTwoWaySwitch.xaml
    /// </summary>
    public partial class xTwoWaySwitch : UserControl
    {
        public event RoutedEventHandler Switched;
        public string First
        {
            get { return tbFirst.Text; }
            set { tbFirst.Text = value; }
        }
        public string Second
        {
            get { return tbSecond.Text; }
            set { tbSecond.Text = value; }
        }
        
        private bool _checked = false;
        public bool Checked
        {
            get { return _checked; }
            set
            {
                _checked = value;
                AnimateSwitch(_checked);
            }
        }

        public xTwoWaySwitch()
        {
            InitializeComponent();
        }
        public void SetChoise(string up, string down)
        {
            tbFirst.Text = up;
            tbSecond.Text = down;
        }

        private void Switch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Checked = !_checked;
            BroadcastEvent();
        }
        private void AnimateSwitch(bool off)
        {
            Storyboard animOff = new Storyboard();
            var anim = new ThicknessAnimation();
            if (off)
            {
                anim.From = new Thickness(2, 2, 2, 2);
                anim.To = new Thickness(2, 22, 2, 2);
            }
            else
            {
                anim.From = new Thickness(2, 22, 2, 2);
                anim.To = new Thickness(2, 2, 2, 2);
            }
            anim.Duration = TimeSpan.FromSeconds(0.25);
            Storyboard.SetTarget(anim, bdrSwitcher);
            Storyboard.SetTargetProperty(anim, new PropertyPath(MarginProperty));
            animOff.Children = new TimelineCollection{anim};
            animOff.Begin();
        }
        private void BroadcastEvent()
        {
            if (Switched !=null) Switched(this, new RoutedEventArgs());
        }
    }
}
