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
    /// Логика взаимодействия для xTestTable.xaml
    /// </summary>
    public partial class xTestTable : UserControl
    {
        public int RowCount
        { get { return xDataGrid.Items.Count; } }


        public xTestTable()
        {
            InitializeComponent();
        }

        public void CreateTable(string[] Headers, string[] Bindings, double ColumnMinWidth)
        {
            
            int count = Headers.Length;
            xDataGrid.Columns.Clear();
            xDataGrid.Items.Clear();

            for (int i = 0; i < count; i++)
            {
                DataGridTextColumn column = new DataGridTextColumn();
                column.Header = Headers[i];
                column.Binding = new Binding(Bindings[i]);
                column.MinWidth = ColumnMinWidth;
                xDataGrid.Columns.Add(column);
            }
        }
        public void AddRow(object row)
        {
            xDataGrid.Items.Add(row);
        }
        public object GetRow(int index)
        {
            return xDataGrid.Items[index];
        }
        
        public void ReplaceRow(int index, object row)
        {
            xDataGrid.Items.RemoveAt(index);
            xDataGrid.Items.Insert(index, row);
        }
    }
}
