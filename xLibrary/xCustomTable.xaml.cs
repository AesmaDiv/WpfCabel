using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// Логика взаимодействия для xCustomTable.xaml
    /// </summary>
    public partial class xCustomTable : UserControl
    {
        #region Переменные
        private int _columns_count = 5; // кол-во столбцов
        private int _rows_count = 3;    // кол-во строк
        private int[] _header_rows = { -1 };

        private double _data_column_width = 70; // ширина ячеей
        private double _header_column_width = 150; // ширина заголовков строк
        private double _row_height = 30;        // высота строк
        private double[] _col_width = new double[0];
        private int[] _col_type = new int[0]; 

        private bool _has_column_header = true;           // есть ли заголовки столбцов
        private bool _has_row_header = true;         // есть ли заголовки строк
        private bool _is_cells_editable = true;      // можно ли редактировать ячейки

        private Brush _table_background = Brushes.Transparent;
        private Brush _table_borderbrush = Brushes.Transparent;

        private Brush _header_background = Brushes.LightGray;//Brushes.DarkBlue;
        private Brush _header_foreground = Brushes.Yellow;
        private Brush _header_borderbrush = Brushes.Gray;
        private Brush _cell_background = Brushes.Blue;
        private Brush _cell_foreground = Brushes.White;
        private Brush _cell_borderbrush = Brushes.Transparent;
        private Brush _selected_cell_background = Brushes.Black;

        private Brush _mouse_over_borderbrush = Brushes.White;
        private Brush _idle_borderbrush = Brushes.Gray;
        private Brush _selected_borderbrush = Brushes.Yellow;

        private Thickness _header_column_cell_borderthickness = new Thickness(0, 1, 0, 1);
        private Thickness _header_row_cell_borderthickness = new Thickness(1, 0, 1, 0);
        private Thickness _cell_borderthickness = new Thickness(0);

        public Thickness _idle_borderthickness = new Thickness(1);
        public Thickness _selected_borderthickness = new Thickness(2);
        


        private string _selected_cell_name = "";
        private string _previous_cell_name = "";
        private string _edited_cell_name = "";

        private ContextMenu _menu = new ContextMenu();
        #endregion

        #region ССЫЛКИ 1
        [Category("xOptions")]
        public bool HasColumnHeader
        {
            get { return _has_column_header; }
            set
            {
                _has_column_header = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public int ColumnCount
        {
            get { return _columns_count; }
            set
            {
                _columns_count = value;
                _col_width = new double[value];
                Rebuild();
            }
        }
        [Category("xOptions")]
        public double DataColumnWidth
        {
            get { return _data_column_width; }
            set
            {
                _data_column_width = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public string ColumnWidthArray
        {
            get { return xFunctions.ArrayToString<double>(_col_width, ","); }
            set
            {
                _col_width = Array.ConvertAll<string, double>(value.Split(','), double.Parse);
                Rebuild();
            }
        }
        [Category("xOptions")]
        public string ColumnsTypeArray
        {
            get { return xFunctions.ArrayToString<int>(_col_type, ","); }
            set
            {
                _col_type = Array.ConvertAll<string, int>(value.Split(','), int.Parse);
                Rebuild();
            }
        }
        [Category("xOptions")]
        public double HeaderColumnWidth
        {
            get { return _header_column_width; }
            set
            {
                _header_column_width = value;
                Rebuild();
            }
        }

        [Category("xOptions")]
        public bool HasRowHeader
        {
            get { return _has_row_header; }
            set
            {
                _has_row_header = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public int RowCount
        {
            get { return _rows_count; }
            set
            {
                _rows_count = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public double RowHeight
        {
            get { return _row_height; }
            set
            {
                _row_height = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public string HeaderRowsIndices
        {
            get { return xFunctions.ArrayToString<int>(_header_rows, ","); }
            set
            {
                _header_rows = Array.ConvertAll<string, int>(value.Split(','), int.Parse);
                Rebuild();
            }
        }

        #endregion

        #region ССЫЛКИ 2
        [Category("xOptions")]
        public Brush TableBackground
        {
            get { return bdrCustomTable.Background; }
            set { bdrCustomTable.Background = value; }
        }
        [Category("xOptions")]
        public Brush TableBorderbrush
        {
            get { return bdrCustomTable.BorderBrush; }
            set { bdrCustomTable.BorderBrush = value; }
        }
        [Category("xOptions")]
        public Thickness TableBorderThickness
        {
            get { return bdrCustomTable.BorderThickness; }
            set { bdrCustomTable.BorderThickness = value; }
        }
        


        [Category("xOptions")]
        public Brush HeaderBackground
        {
            get { return _header_background; }
            set
            {
                _header_background = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public Brush HeaderForeground
        {
            get { return _header_foreground; }
            set
            {
                _header_foreground = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public Brush HeaderBorderbrush
        {
            get { return _header_borderbrush; }
            set
            {
                _header_borderbrush = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public Thickness HeaderColumnCellBorderThickness
        {
            get { return _header_column_cell_borderthickness; }
            set
            {
                _header_column_cell_borderthickness = value;
                Rebuild();
            }

        }
        [Category("xOptions")]
        public Thickness HeaderRowCellBorderThickness
        {
            get { return _header_row_cell_borderthickness; }
            set
            {
                _header_row_cell_borderthickness = value;
                Rebuild();
            }

        }
        

        [Category("xOptions")]
        public Brush CellBackground
        {
            get { return _cell_background; }
            set
            {
                _cell_background = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public Brush CellForeground
        {
            get { return _cell_foreground; }
            set
            {
                _cell_foreground = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public Brush CellBorderbrush
        {
            get { return _cell_borderbrush; }
            set
            {
                _cell_borderbrush = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public Thickness CellBorderThickness
        {
            get { return _cell_borderthickness; }
            set
            {
                _cell_borderthickness = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public bool IsCellsEditable
        {
            get { return _is_cells_editable; }
            set
            {
                _is_cells_editable = value;
                Rebuild();
            }
        }
        [Category("xOptions")]
        public Brush SelectedCellBackground
        {
            get { return _selected_cell_background; }
            set
            {
                _selected_cell_background = value;
            }
        }


        #endregion

        public xCustomTable()
        {
            InitializeComponent();
            Rebuild();
        }

        #region Создание таблилы
        public void Rebuild()
        {
            stkMain.Children.Clear();            

            int count = _has_column_header ? _rows_count + 1 : _rows_count;

            for (int i = 0; i < count; i++)
                stkMain.Children.Add(CreateRow(i, (_has_column_header && i == 0)));
        }
        
        private Border CreateRow(int index, bool is_header_row)
        {
            if (_header_rows.Contains<int>(index)) is_header_row = true;
            
            StackPanel stk = new StackPanel() { Orientation = Orientation.Horizontal };
            Border result = new Border()
            {
                BorderBrush = is_header_row ? _header_borderbrush : _cell_borderbrush,
                BorderThickness = is_header_row ? _header_column_cell_borderthickness : _cell_borderthickness
            };
            

            int count = _columns_count;
            int[] types = _col_type;

            if (_has_row_header)
            {
                count++;
                types = xFunctions.CombineArrays<int>(new int[] { 0 }, types);
            }
            
            for (int i = 0; i < count; i++)
                stk.Children.Add(CreateCell(index,
                                               i,
                                               _has_row_header && i == 0,
                                               is_header_row,
                                               types.Length > i ? types[i] : 0));
            result.Child = stk;
            return result;
        }
        private Border CreateCell(int row_index, int column_index, bool is_row_header, bool is_header_cell, int cell_type)
        {
            if (is_row_header) is_header_cell = true;
            if (is_header_cell) cell_type = 0;

            UnregisterCellName(row_index, column_index);

            string bdr_cell_name = is_header_cell ? "bdrHeaderCell_" : "bdrDataCell_";
            string txt_cell_name = is_header_cell ? "txtHeaderCell_" : "txtDataCell_";

            bdr_cell_name += row_index.ToString() + "_" + column_index.ToString();
            txt_cell_name += row_index.ToString() + "_" + column_index.ToString();

            if (_has_row_header) column_index--;
            double width = _header_column_width;
            if ((!is_row_header) && (column_index < _col_width.Length))
                width = _col_width[column_index] == 0 ? _data_column_width : _col_width[column_index];

            Border result = new Border()
            {
                Name = bdr_cell_name,
                Width = width,
                Height = _row_height,
                Background = is_header_cell ? _header_background : _cell_background,
                BorderBrush = is_header_cell ? _header_borderbrush : _cell_borderbrush,
                BorderThickness = is_row_header ? _header_row_cell_borderthickness : _cell_borderthickness,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            //result.MouseEnter += bdrCell_MouseEnter;
            //result.MouseLeave += bdrCell_MouseLeave;
            //result.MouseUp += bdrCell_MouseUp;
            //result.KeyDown += bdrCell_KeyDown;
            //result.MouseRightButtonUp += bdrCell_MouseRightButtonUp;

            //if (this.FindName(bdr_cell_name) != null) this.UnregisterName(bdr_cell_name);
            //this.RegisterName(bdr_cell_name, result);
            //if (this.FindName(txt_cell_name) != null) this.UnregisterName(txt_cell_name);

            switch (cell_type)
            {
                case 0:
                    var tbl = new TextBlock()
                    {
                        Name = txt_cell_name,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Background = Brushes.Transparent,
                        Foreground = _cell_foreground
                    };
                    if (this.FindName(txt_cell_name) != null) this.UnregisterName(txt_cell_name);
                    this.RegisterName(tbl.Name, tbl);
                    result.Child = tbl;
                    break;
                case 1:
                    var tbx = new TextBox()
                    {
                        Name = txt_cell_name,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        BorderBrush = Brushes.Transparent,
                        Background = Brushes.Transparent,
                        Foreground = _cell_foreground
                    };
                    tbx.KeyUp += bdrCell_KeyPressed;
                    if (this.FindName(txt_cell_name) != null) this.UnregisterName(txt_cell_name);
                    this.RegisterName(tbx.Name, tbx);
                    result.Child = tbx;
                    break;
                case 2:
                    var cbx = new ComboBox()
                    {
                        Name = txt_cell_name,
                        VerticalAlignment = VerticalAlignment.Center,
                        BorderBrush = Brushes.Transparent,
                        Background = Brushes.Transparent,
                        Foreground = Brushes.Black
                    };
                    cbx.KeyUp += bdrCell_KeyPressed;
                    if (this.FindName(txt_cell_name) != null) this.UnregisterName(txt_cell_name);
                    this.RegisterName(cbx.Name, cbx);
                    result.Child = cbx;
                    break;
            }

            return result;
        }
        private void UnregisterCellName(int row_index, int column_index)
        {
            string[] names = new string[]
            {
                "bdrHeaderCell_" + row_index.ToString() + "_" + column_index.ToString(),
                "txtHeaderCell_" + row_index.ToString() + "_" + column_index.ToString(),
                "bdrDataCell_" + row_index.ToString() + "_" + column_index.ToString(),
                "txtDataCell_" + row_index.ToString() + "_" + column_index.ToString()
            };
            for(int i=0;i < names.Length; i++)
                if(this.FindName(names[i]) != null) this.UnregisterName(names[i]);
        }
        #endregion

        #region Обработка событий мыши
        void bdrCell_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Border bdr = (Border)sender;
            //if (bdr == null) return;
            //bdr.Background = _selected_cell_background;
            //_selected_cell_name = bdr.Name;

            //bdr = (Border)this.FindName(_previous_cell_name);
            //_previous_cell_name = _selected_cell_name;
            //if (bdr == null) return;
            //bdr.Background = _cell_background;
        }
        void bdrCell_MouseLeave(object sender, MouseEventArgs e)
        {
            Border bdr = (Border)sender;
            if (bdr == null) return;
            bdr.BorderBrush = _idle_borderbrush;

        }
        void bdrCell_MouseEnter(object sender, MouseEventArgs e)
        {
            Border bdr = (Border)sender;
            if (bdr == null) return;
            bdr.BorderBrush = _mouse_over_borderbrush;
        }
        void bdrCell_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_is_cells_editable) return;
            Border bdr = (Border)sender;
            if (bdr == null) return;
            _edited_cell_name = bdr.Name.Replace("bdr", "tb");
            TextBlock tb = (TextBlock)this.FindName(_edited_cell_name);
            if (tb == null) return;

        }
        #endregion

        #region Обработка нажатия клавиш
        void bdrCell_KeyPressed(object sender, KeyEventArgs e)
        {
            NavigateCells(sender, e.Key);
        }
        #endregion
        #region Задание и получение значений ячейкам и залоговкам
        // *************************************************************************************************************** //
        public string GetTableHeader()
        {
            return GetCellValue("txtHeaderCell_0_0");
        }
        public void SetTableHeader(string value)
        {
            SetCellValue("txtHeaderCell_0_0", value);
        }
        // *************************************************************************************************************** //
        public string GetRowHeader(int row)
        {
            if (_has_column_header) row++;
            return GetCellValue("txtHeaderCell_" + row + "_0");
        }
        public void SetRowHeader(int row, string value)
        {
            if (_has_column_header) row++;
            SetCellValue("txtHeaderCell_" + row + "_0", value);
        }
        // *************************************************************************************************************** //
        public string GetColumnHeader(int column)
        {
            if (_has_row_header) column++;
            return GetCellValue("txtHeaderCell_0_" + column);
        }
        public void SetColumnHeader(int column, string value)
        {
            SetColumnHeader(0, column, value);
        }
        public void SetColumnHeader(int row, int column, string value)
        {
            if (_has_row_header) column++;
            SetCellValue("txtHeaderCell_" + row + "_" + column, value);
        }
        // *************************************************************************************************************** //
        private void NavigateCells(object obj, Key key)
        {
            try
            {
                string cell_name = (obj as Control).Name;

                string[] temp = cell_name.Split('_');
                if (temp.Length != 3) return;

                int row = int.Parse(temp[1]);
                int col = int.Parse(temp[2]);

                object next_cell = null;
                switch(key)
                {
                    case Key.Enter:
                        next_cell = this.FindName(temp[0] + "_" + (row + 1) + "_" + col);
                        if (next_cell == null) next_cell = FindName(temp[0] + "_" + (row + 2) + "_" + col);
                        if (next_cell == null) next_cell = FindName(temp[0] + "_" + (HasColumnHeader ? 1 : 0) + "_" + (col+1));
                        break;
                    case Key.Up:
                        next_cell = FindName(temp[0] + "_" + (row - 1) + "_" + col);
                        if (next_cell == null) next_cell = FindName(temp[0] + "_" + (row - 2) + "_" + col);
                        break;
                    case Key.Down:
                        next_cell = FindName(temp[0] + "_" + (row + 1) + "_" + col);
                        if (next_cell == null) next_cell = FindName(temp[0] + "_" + (row + 2) + "_" + col);
                        break;
                    case Key.Left:
                        next_cell = FindName(temp[0] + "_" + row + "_" + (col - 1));
                        break;
                    case Key.Right:
                        next_cell = FindName(temp[0] + "_" + row + "_" + (col + 1));
                        break;
                }
                if (next_cell == null) return;
                if (next_cell is TextBox) (next_cell as TextBox).Focus();
                if (next_cell is ComboBox) (next_cell as ComboBox).Focus();

            }
            catch (Exception ex) { }

        }
        private void SettingFloatingHeaderCell(string name)
        {
            var tb = this.FindName(name);
            if (tb == null) return;

            if (tb is TextBlock)
            {
                ((TextBlock)tb).Background = Brushes.LightGray;//_header_background;
                //((TextBlock)tb).Foreground = _cell_foreground;
                ((TextBlock)tb).IsEnabled = false;                
                //((TextBlock)tb).FontWeight = FontWeights.Bold;
            }

            if (tb is TextBox)
            {
                ((TextBox)tb).Background = Brushes.LightGray;//_header_background;
                //((TextBox)tb).Foreground = _cell_foreground;
                ((TextBox)tb).IsEnabled = false;
                //((TextBox)tb).FontWeight = FontWeights.Bold;
            }

            if (tb is Border)
            {
                ((Border)tb).Background = Brushes.LightGray;
                ((Border)tb).IsEnabled = false;
            }
        }        
        
        // *************************************************************************************************************** //
        public object GetCellObject(int row, int column)
        {
            return this.FindName("txtDataCell_" + row + "_" + column);
        }
        public string GetCellValue(int row, int column)
        {
            if (_has_column_header) row++;
            if (_has_row_header) column++;
            return GetCellValue("txtDataCell_" + row + "_" + column);
        }
        public void SetCellValue(int row, int column, string value)
        {
            if (_has_column_header) row++;
            if (_has_row_header) column++;
            SetCellValue("txtDataCell_" + row + "_" + column, value);
        }
        // *************************************************************************************************************** //
        public string GetSelectedCellValue()
        {
            Border bdr = (Border)this.FindName(_selected_cell_name);
            if (bdr == null) return "-error-";
            TextBox tb = (TextBox)bdr.Child;
            if (tb == null) return "-error-";
            return tb.Text;
        }
        public void SetSelectedCellValue(string value)
        {
            Border bdr = (Border)this.FindName(_selected_cell_name);
            if (bdr == null) return;
            TextBox tb = (TextBox)bdr.Child;
            if (tb == null) return;
            tb.Text = value;
        }
        public void SetCellDefaultValue(string default_value)
        {
            for(int col=0;col<_columns_count; col++)
                for(int row=0; row<_rows_count;row++)
                    SetCellValue(row,col,default_value);
        }
        // *************************************************************************************************************** //
        #endregion

        #region Изменение размера ячеек
        public void ResizeCells(double cell_size)
        {
            Border bdr;
            for (int col = 0; col < _columns_count; col++)
            {
                bdr = (Border)this.FindName("bdrColumn_" + col);
                if (bdr != null) bdr.Width = cell_size;
                for (int row = 0; row < _rows_count; row++)
                {
                    bdr = (Border)this.FindName("bdrCell_" + row + "_" + col);
                    if (bdr != null) bdr.Width = cell_size;
                }
            }
        }
        public void FitTableToSize(double size)
        {
            ResizeCells((size - _header_column_width) / _columns_count);
        }
        public void SetColumnWidth(int column, double width)
        {
            if (column < _col_width.Length) return;
            _col_width[column] = width;
        }
        #endregion

        private string GetCellValue(string name)
        {
            var cell = this.FindName(name);
            if (cell == null) return "";

            if (cell is TextBlock) return ((TextBlock)cell).Text;
            else if (cell is TextBox) return ((TextBox)cell).Text;
            else if (cell is ComboBox) return ((ComboBox)cell).Text;
            return "";
        }
        private void SetCellValue(string name, string value)
        {
            var cell = this.FindName(name);
            if (cell == null) return;

            if (cell is TextBlock) ((TextBlock)cell).Text = value;
            else if (cell is TextBox) ((TextBox)cell).Text = value;
            else if (cell is ComboBox) ((ComboBox)cell).Text = value;
        }

        // rona
        public void SettingInProtocol_Cells(Brush background_header, Brush background_cell, bool paint_borderAround, bool paint_borderHeaderRow, bool paint_borderHeaderColumn, bool floating_headerRow, int row_floatingHeader)
        {
            //    try
            //    {
            //        int count_rows = _rows_count;
            //        int count_columns = _columns_count;

            //        if (_has_column_header) count_rows = _rows_count + 1;
            //        if (_has_row_header) count_columns = _columns_count + 1;

            //        for (int i = 0; i < count_rows; i++)
            //        {
            //            for (int j = 0; j < count_columns; j++)
            //            {
            //                // заголовок 

            //                var tb = this.FindName("txtHeaderCell_" + i + "_" + j);

            //                if (tb is TextBlock)
            //                {
            //                    ((TextBlock)tb).Background = background_header;
            //                    ((TextBlock)tb).FontWeight = FontWeights.Bold;
            //                }

            //                var brd = this.FindName("bdrHeaderCell_" + i + "_" + j);

            //                if (brd is Border)
            //                {
            //                    ((Border)brd).BorderThickness = new Thickness(0);
            //                    ((Border)brd).Background = background_header;
            //                }

            //                // ячейка значения

            //                tb = this.FindName("txtDataCell_" + i + "_" + j);

            //                if (tb is TextBlock)
            //                {
            //                    ((TextBlock)tb).Background = background_cell;
            //                    ((TextBlock)tb).FontWeight = FontWeights.Regular;
            //                }

            //                brd = this.FindName("bdrDataCell_" + i + "_" + j);

            //                if (brd is Border)
            //                {
            //                    ((Border)brd).BorderThickness = new Thickness(0);
            //                }

            //                brd = null;
            //                tb = null;
            //            }
            //        }

            //        // рамка заголовков столбцов

            //        if (paint_borderHeaderColumn && _has_column_header)
            //        {
            //            int bottom = Convert.ToInt32(_rows_count * _row_height + 2);

            //            Border borderHeaderColumn = new Border()
            //            {
            //                BorderThickness = new Thickness(2),
            //                BorderBrush = Brushes.Black,
            //                Margin = new Thickness(0, 0, 0, bottom),
            //                Width = grdCustomTable.Width,
            //                Height = _row_height
            //            };

            //            grdCustomTable.Children.Add(borderHeaderColumn);
            //        }

            //        // рамка заголовков строк

            //        if (paint_borderHeaderRow && _has_row_header)
            //        {

            //            int right = 0;

            //            for (int i = 0; i < _columns_count; i++)
            //            {
            //                right = right + Convert.ToInt32(_col_width[i]);
            //            }

            //            Border borderHeaderRow = new Border()
            //            {
            //                BorderThickness = new Thickness(2),
            //                BorderBrush = Brushes.Black,
            //                Margin = new Thickness(0, 0, right, 0),
            //                Width = _header_column_width,
            //                Height = grdCustomTable.Height
            //            };

            //            grdCustomTable.Children.Add(borderHeaderRow);
            //        }

            //        // внешняя рамка

            //        if (paint_borderAround)
            //        {
            //            Border borderAround = new Border()
            //            {
            //                BorderThickness = new Thickness(2),
            //                BorderBrush = Brushes.Black,
            //                Width = grdCustomTable.Width,
            //                Height = grdCustomTable.Height
            //            };

            //            grdCustomTable.Children.Add(borderAround);
            //        }
            //    }
            //    catch (Exception ex) { }
            //}       
        } 
    }
}
