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
using System.ComponentModel;

namespace xLibrary
{
	/// <summary>
	/// Interaction logic for xTestButton.xaml
	/// </summary>
	public partial class xTestButton : UserControl, INotifyPropertyChanged
	{
        bool _status;
		public bool Status
		{
			get { return _status; }
			set 
			{
				_status = value;
                if(!_status) this.ToolTip = "Запустить тест";
                else this.ToolTip = "Прекратить тест";
			    NotifyStatusChanged("Status");
            }
		}
        bool _active;
        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                if (!_active) 
                    if(!_status) this.ToolTip = "Чтобы запустить тест необходимо указать Тип";
                    else this.ToolTip = "Пожалуйста подождите...";
                else if (!_status) this.ToolTip = "Запустить тест";
                     else this.ToolTip = "Прекратить тест";
                NotifyStatusChanged("Active");
                              
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyStatusChanged(string propertyName)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

		public xTestButton()
		{
			this.InitializeComponent();
		}
		public void xButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            Status = !Status;
		}
	}
}