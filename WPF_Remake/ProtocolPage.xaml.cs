using System.Windows.Controls;
using System.Windows.Media;

namespace WPF_Try
{
    /// <summary>
    /// Логика взаимодействия для ProtocolPage.xaml
    /// </summary>
    public partial class ProtocolPage : Page
    {
        
        public System.Drawing.Size ChartSize
        { get { return new System.Drawing.Size((int)imgChart.Width, (int)imgChart.Height); } }

        public ProtocolPage()
        {
            InitializeComponent();
        }

        public object GetControl(string name)
        {
            return this.FindName(name);
        }
        public void SetChart(System.Drawing.Bitmap bmp)
        {
            imgChart.Source = xLibrary.xFunctions.ToBitmapSource(bmp);
        }
        public void ShowStructureData(bool state)
        {
            stkStructData.Visibility = state ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        private void tableROhm_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
