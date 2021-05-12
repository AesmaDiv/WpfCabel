using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Логика взаимодействия для xValueTable.xaml
    /// </summary>
    public partial class xValueTable : UserControl
    {
        public event RoutedEventHandler ValueChanged;
        string[] _names;
        string[] _headers;
        bool[] _editable;
        public xValueTable()
        {
            InitializeComponent();
        }
        public void CreateTable(string[] names, string[] headers)
        {
            int array_length = names.Length;
            _names = names;
            _headers = headers;
            _editable = new bool[names.Length];

            CreateTable();
        }
        public void CreateTable()
        {
            xDataGrid.Items.Clear();
            xDataGrid.Columns.Clear();
            
            Style header_style = new Style(typeof(DataGridRowHeader));
            header_style.Setters.Add(new Setter(DataGridRowHeader.BackgroundProperty, Brushes.LightGray));
            Style cell_style = new Style(typeof(TextBlock));
            cell_style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
            cell_style.Setters.Add(new Setter(TextBlock.BackgroundProperty, Brushes.Transparent));

            DataGridTextColumn col = new DataGridTextColumn();
            col.Width = 70;
            col.Binding = new Binding("Text");
            col.ElementStyle = cell_style;
            xDataGrid.Columns.Add(col);
            
            
            for (int i = 0; i < _names.Length; i++)
            {
                TextBlock row = new TextBlock();
                row.Name = _names[i];
                row.Text = "";
                //row.IsReadOnly = !editable[i];
                
                DataGridRow dgr = new DataGridRow();
                dgr.Name = _names[i];
                TextBlock tb = new TextBlock();
                tb.Text = _headers[i];
                tb.TextAlignment = TextAlignment.Right;
                tb.FontWeight = FontWeights.Bold;
                tb.Width = 170;
                dgr.Header = tb;

                if (dgr.Name == "")
                {
                    dgr.HeaderStyle = header_style;
                    dgr.Background = Brushes.LightGray;
                    dgr.Height = 10;
                }
                dgr.Item = row;
                xDataGrid.Items.Add(dgr);
            }
        }

        public void SetValue(string name, string value)
        {
            try
            {
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                { 
                    int index = Array.FindIndex<string>(_names, str => str == name);
                    DataGridRow dgr = xDataGrid.Items[index] as DataGridRow;
                    if (dgr != null)
                    {
                        TextBlock tb = dgr.Item as TextBlock;
                        if (tb != null)  tb.Text = value;
                    }
                }));
            }
            catch (Exception ex) { }
        }
        public string GetValue(string name)
        {
            try
            {
                int index = Array.FindIndex<string>(_names, str => str == name);
                DataGridRow dgr = xDataGrid.Items[index] as DataGridRow;
                if (dgr != null)
                {
                    TextBlock tb = dgr.Item as TextBlock;
                    if (tb != null) return tb.Text;
                }
            }
            catch (Exception ex) { }

            return "Not Found";
        }
    }
}
