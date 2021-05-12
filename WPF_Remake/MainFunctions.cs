using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Printing;
using xLibrary;
using System.Runtime.InteropServices;
using System.Drawing.Printing;

namespace WPF_Try
{
    class MainFunctions
    {               
        #region ПОЛЯ ОПИСАНИЯ
        // ******************************************************************************************************************************* //
        // ******************************************************************************************************************************* //
        public static void CreateTestDescription(ref WrapPanel wrp, string[] names, string[] headers, bool[] types, bool[] is_editable)
        {
            // Очищаем панель
            wrp.Children.Clear();
            // Получаем число элементов
            int count = names.Length;
            // Задаём высоту панели (делим число элементов на 2, округляем в большую сторону и умножаем на вывосту элемента(30))
            int wrp_height = 25 * Convert.ToInt16(Math.Ceiling((float)count / 2.0));
            wrp.Height = wrp_height;

            for (int i = 0; i < count; i++)
            {
                // Создаем элемент поля и добавляем в панель
                wrp.Children.Add(CreateDataField(names[i], headers[i], types[i], is_editable[i]));
            }
            //FillDataFields_DefaultValue(ref wrp);
        }
        public static xDataField CreateDataField(string name, string header, bool type, bool is_editable)
        {
            // Создаем элемент поля и...
            xDataField result = new xLibrary.xDataField()
            {
                Type = type ? xDataField.FieldType.ComboBox : xDataField.FieldType.TextBox,
                //...задаём его имя
                Name = name,
                Height = 25,
                FieldWidth = 200,
                Margin = new Thickness(40, 0, 40, 0),
                Visibility = (name == "") ? Visibility.Hidden : Visibility.Visible,
                //...задаём его заголовок
                Header = header,
                //...задаём его редактируемость
                IsEditable = is_editable
            };
            //...задаём его тип
            if (type)
            {
                //...задаем обработчик события
                result.ComboBox_SelectionChanged += DataField_TextChanged;
                if (name == "CabelSort") LoadSortaments(ref result);
                else FillComboBox(ref result);
            }
            if ((name == "CabelSort") || (name == "Length")) Vars.mainWindow.RegisterName(name, result);
            return result;
        }
        // ******************************************************************************************************************************* //
        public static string[] GetTestDestriptionValues(WrapPanel wrp, string[] names)
        {
            string[] result = new string[names.Length];
            for(int i=0;i<names.Length;i++)
                result[i] = GetTestDestriptionValue(wrp, names[i]);

            return result;
        }
        public static string   GetTestDestriptionValue(WrapPanel wrp, string name)
        {
            foreach (xLibrary.xDataField df in wrp.Children)
                if (df.Name == name) return df.Value;

            return "";
        }
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) child = GetVisualChild<T>(v);
                if (child != null) break;
            }
            return child;
        }
        public static DataGridCell GetCell(DataGrid grid, DataGridRow row, int column)
        {
            if (row != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null)
                {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell result = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return result;
            }
            return null;
        }
        // ******************************************************************************************************************************* //
        public static void FillTestList(ref xTestList testlist)
        {
            testlist.SetSource(null);
            Vars.TestListRow[] rows = Vars.Access.GetTestListRows<Vars.TestListRow>(Vars.ListRow_Bindings);
            testlist.SetSource(rows);
        }        
        public static void LoadSortaments(ref xDataField df, [Optional] bool is_default)
        {
            string[] elements = new string[0];
            if (File.Exists(Vars.path_Sortament))
                try
                {
                    elements = File.ReadAllLines(Vars.path_Sortament);
                    elements = elements.SkipWhile<string>(str => str.StartsWith("//")).ToArray<string>();
                    for (int i = 0; i < elements.Length; i++)
                        elements[i] += " мм²";

                    df.ItemsSource = elements;
                }
                catch (Exception ex) { }
        }

        public static void FillComboBox(ref xDataField df)
        {
            string[] values = Vars.Access.GetUniqItemsList(df.Name);
            //доп.проверка на уникальность строк
            values = values.Distinct<string>().ToArray<string>();
            
            df.ItemsSource = values;
        }
        public static void FillDataFields_CurrentValues(ref WrapPanel wrp)
        {
            foreach (xLibrary.xDataField df in wrp.Children)
                FillDataField_CurrentValue(df);
        }
        public static void FillDataField_CurrentValue(xDataField df)
        {
            string value = Vars.CurrentCabel.GetItemValue(df.Name);
            if (df.Type == xLibrary.xDataField.FieldType.ComboBox)
            {
                df.ComboBox.SelectedValue = "";
                if (df.Name != "CabelSort") FillComboBox(ref df);
                df.ComboBox.SelectedValue = value;
            }
            else df.TextBox.Text = Vars.CurrentCabel.GetItemValue(df.Name);

            TextBlock tb = Vars.protocol.FindName("txt" + df.Name) as TextBlock;
            if (tb != null) tb.Text = value;

        }
        // ******************************************************************************************************************************* //
        public static bool SaveCurrentCabel_Data(WrapPanel wrp, string comment)
        {
            if (Vars.CurrentCabel == null) return false;

            try
            {
                string[] fields = xFunctions.CombineArrays<string>(Vars.DataField_Names, new string[] { "Comment" });
                string[] values = xFunctions.CombineArrays<string>(GetTestDestriptionValues(wrp, Vars.DataField_Names), new string[] { comment });

                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i] == "") continue;
                    Vars.CurrentCabel.SetItemValue(fields[i], values[i]);
                }
            }
            catch (Exception ex) { return false; }

            return true;
        }
        
        public static bool ManualSave_ROhm()
        {
            try
            {
                float ab = float.Parse(Vars.rohm_table.GetCellValue(1, 3));
                float bc = float.Parse(Vars.rohm_table.GetCellValue(2, 3));
                float ca = float.Parse(Vars.rohm_table.GetCellValue(3, 3));
                Vars.CurrentCabel.ROhm_AddValue(ab, CabelData.ROhmFase.AB);
                Vars.CurrentCabel.ROhm_AddValue(bc, CabelData.ROhmFase.BC);
                Vars.CurrentCabel.ROhm_AddValue(ca, CabelData.ROhmFase.CA);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        // ******************************************************************************************************************************* //
        public static void CreateTable_Hipot(xCustomTable table, bool has_row_headers, int row_count)
        {
            table.RowCount = row_count + 1;

            for (int i = 0; i < 4; i++)
                table.SetColumnHeader(i, Vars.HipotTable_Headers[i]);

            if (has_row_headers)
            {
                table.SetTableHeader("t,мин");

                for (int i = 0; i <= row_count; i++)
                    table.SetRowHeader(i, /*i == 0 ? "0**" :*/ (i+1).ToString());
            }
        }
        public static void CreateTable_ROhm(xCustomTable table)
        {
            for (int i = 0; i < Vars.ROhmTable_RowHeaders.Length; i++)
                table.SetRowHeader(i, Vars.ROhmTable_RowHeaders[i]);
            for (int i = 0; i < Vars.ROhmTable_ColHeaders.Length; i++)
                table.SetColumnHeader(i, Vars.ROhmTable_ColHeaders[i]);
        }
        public static void CreateTable_Struct(xCustomTable table)
        {
            for (int i = 0; i < Vars.StructCabelTable_Headers_Rows.Length; i++)
                table.SetRowHeader(i, Vars.StructCabelTable_Headers_Rows[i]);
            for (int i = 0; i < Vars.StructCabelTable_Headers_Columns.Length; i++)
                table.SetColumnHeader(i, Vars.StructCabelTable_Headers_Columns[i]);
            for (int i = 0; i < Vars.StructCabelTable_FloatingHeaders_Columns.Length; i++)
                table.SetColumnHeader(7, i, Vars.StructCabelTable_FloatingHeaders_Columns[i]);

            string[] cb_source = new string[] { "", "Новый", "Б/У" };
            for (int i = 0; i < table.RowCount + 1; i++)
            {
                ComboBox cb = table.GetCellObject(i, 7) as ComboBox;
                if (cb == null) continue;
                cb.ItemsSource = cb_source;
            }
        }
        public static void CreateTable_PI(xCustomTable table)
        {
            for (int i = 0; i < Vars.PITable_Headers_Rows.Length; i++)
                table.SetRowHeader(i, Vars.PITable_Headers_Rows[i]);
            for (int i = 0; i < Vars.PITable_Headers_Columns.Length; i++)
                table.SetColumnHeader(i, Vars.PITable_Headers_Columns[i]);
        }
        public static void SetProtocolCellStyle(xCustomTable table)
        {
            table.SettingInProtocol_Cells(Brushes.Transparent, Brushes.Transparent, true, true, true, false, 0);
        }
        public static void ShowPI(bool state, CabelData.HipotFase fase, CabelData.HipotData hipot_data)
        {
            StackPanel stk = null;
            TextBlock txt = null;

            switch(fase)
            {
                case CabelData.HipotFase.A:
                    stk = Vars.protocol.FindName("stkPiA") as StackPanel;
                    txt = Vars.protocol.FindName("txtPiA") as TextBlock;
                    break;
                case CabelData.HipotFase.B:
                    stk = Vars.protocol.FindName("stkPiB") as StackPanel;
                    txt = Vars.protocol.FindName("txtPiB") as TextBlock;
                    break;
                case CabelData.HipotFase.C:
                    stk = Vars.protocol.FindName("stkPiC") as StackPanel;
                    txt = Vars.protocol.FindName("txtPiC") as TextBlock;
                    break;
            }

            if (stk != null) stk.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
            if (state)
            {
                float r_first = GetR_FromHipotData(hipot_data, secs => secs == 60);
                float r_last =  GetR_FromHipotData(hipot_data, secs => secs == Vars.TestTime_Long * 60);

                if (txt != null) txt.Text = ((r_first < 0) || (r_last < 0)) ? "-.---" : (r_last / r_first).ToString("0.000");
            }
        }
        // ******************************************************************************************************************************* //
        // ******************************************************************************************************************************* //
        #endregion
        
        public static bool Check_HipotVoltage()
        {
            if (Vars.hipot_params.OutputVoltage == 0)
            {
                MainFunctions.ShowMessage("Hipot", "Укажите подаваемое напряжение.", false);
                MainFunctions.Log("Hipot - остановка. Не указано подаваемое напряжение.");
                return false;
            }
            Vars.Test.Hipot.OutputVoltage = Vars.hipot_params.OutputVoltage == 12 ? 0 : 1;
            //double d = 0;
            //progressVoltage.Maximum = double.TryParse(comboVoltage.SelectedValue.ToString(), out d) ? d : 12;
            //progressVoltage.Value = progressVoltage.Maximum;
            return true;
        }
        public static bool Check_HipotFase()
        {
            if (Vars.Test.Hipot.ActiveFase == CabelData.HipotFase.None)
            {
                MainFunctions.ShowMessage("Hipot", "Выберите фазу для испытания.", false);
                MainFunctions.Log("Hipot - остановка. Не выбрана активная фаза.");
                return false;
            }

            return true;
        }
        public static bool Check_HipotDoorClosed()
        {
            bool is_closed = false;
            return true;

            xEquipment.xAdam5000TCP.Settings.Info info = Vars.AdamTCP.ChannelInfos.GetChannel("Door");
            is_closed = Vars.AdamTCP.StreamData.GetChannelValue_Digital(info.Slot, info.Channel);
            Vars.AdamTCP.SetChannelValue_Digital("Hipot", is_closed);

            if (!is_closed) Vars.mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                ShowMessage("ВНИМАНИЕ!!!", "Открыта калитка!", false)));

            return is_closed;
        }
        #region ПРОТОКОЛ

        // Сформировать и распечатать протокол
        public static void PrintProtocol()
        {
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() != true) return;
            Vars.protocol.RenderTransform = new TranslateTransform(-200, 10);// (-280, 10);
            dialog.PrintVisual(Vars.protocol, "Printing protocol");
            Vars.protocol.RenderTransform = new TranslateTransform(0,0);

        }
        #endregion

        #region EXCEL GRAPH TRY
        // ******************************************************************************************************************************* //
        // ******************************************************************************************************************************* //
        public static void DrawProtocol(ref Image img, System.Drawing.Size size)
        {
            
        }
        #endregion
        // ******************************************************************************************************************************* //
        private static float GetR_FromHipotData(CabelData.HipotData data, Predicate<int> time_index_predicate)
        {
            if (data.Time.Length == 0) return -1;
            if (data.Time.Max<int>() < Vars.TestTime_Long * 60) return -1;
            int index = Array.FindIndex<int>(data.Time, time_index_predicate);
            if (index < 0) return -1;

            return data.Voltage[index] / data.Current[index];
        }
        
        private static void DataField_TextChanged(object sender, RoutedEventArgs e)
        {
            xLibrary.xDataField df = sender as xLibrary.xDataField;
            switch (df.Name)
            {
                case "CabelSort":
                    if (df.Value == "") return;
                    //Vars.CurrentCabel.SetItemValue("CabelSort", df.Value);
                    //CabelData.Sortament sort = Array.Find<CabelData.Sortament>(Vars.sortaments, item => item.Name == df.Value);
                    //Vars.CurrentCabel.CabelSort = sort;
                    break;
                #region
                //case "Producer":
                //    MessageBox.Show("Выбран элемент Заказчик: " + cb.SelectedValue);
                //    // Если значение текущего выбора не пустое...
                //    if ((cb.SelectedValue != null))
                //        //...получаем список уникальных элементов типов из таблицы типов для выбранного производителя
                //        cbType.ItemsSource = xDB.GetUniqItemsList("Type_Name", "Type", "Producer", cb.SelectedValue.ToString());
                //    // В противном случае получаем список всех уникальных элементов типов из таблицы типов
                //    else cbType.ItemsSource = xDB.GetUniqItemsList("Type_Name", "Type");
                //    break;
                #endregion
            }
            //if (Vars.CurrentPump == null) return;
            //Vars.CurrentPump.SetItemValue(df.Name, df.Value);

        }
        public static MessageBoxResult ShowMessage(string title, string message, bool YesNo)
        {
            xMessageWindow msg = new xMessageWindow()
            {
                Title = title,
                OkCancel = YesNo,
                Content = new TextBlock()
                {
                    Text = message,
                    FontSize = 26,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };
            msg.Owner = Vars.mainWindow;

            return msg.Show();
        }
        public static void Log(string message)
        {
            string time_format = "HH:mm:ss.ff";
            if (Vars.journal != null) Vars.journal.Add(message, time_format);
            Vars.mainWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
               ComboBox cb = (ComboBox)Vars.mainWindow.FindName("cbLog");
               if (cb != null) cb.Items.Add(DateTime.Now.ToString(time_format) + ":: " + message);
            }));
            Console.WriteLine(message);
        }
        // ******************************************************************************************************************************* //
    }
}
