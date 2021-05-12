using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using xLibrary;


namespace WPF_Try
{
    public partial class MainWindow : Window
    {
        private Timer _tmr_memory_clean;
        private Timer _tmr_display_time;
        private DateTime _test_start_time;

        private bool _is_Hipot_ended = true;

        int _hipot_package_count = 0;
        int _hipot_i_update_count = 0;
        int _hipot_u_update_count = 0;


        private string GetTestInfoValue() { return null; }
        private WrapPanel wrapCurrentValues = new WrapPanel();

        private enum UserInterfaceMode { TestList, TestDesc, TestView, Locked };

        public string path = "d:\\work\\setting.xml";                           // путь к xml-файлу

        /* ************************************************************************************************ */
        /* Инициализация, заргузка, закрытие Главного окна                                                  */
        /* ************************************************************************************************ */
        #region Собственно..
        public MainWindow()
        {
            InitializeComponent();            
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Vars.mainWindow = this;
            
            MainFunctions.Log("********");
            MainFunctions.Log("Запуск программы.");

            

            Vars.rohm_table = tableROhm;
            Vars.rohm_table_protocol = Vars.protocol.FindName("tableROhm") as xCustomTable;
            Vars.hipot_tables = new xCustomTable[]
            {
                tableHipotA,
                tableHipotB,
                tableHipotC
            };
            Vars.hipot_tables_protocol = new xCustomTable[]
            {
                Vars.protocol.FindName("tableHipotA") as xCustomTable,
                Vars.protocol.FindName("tableHipotB") as xCustomTable,
                Vars.protocol.FindName("tableHipotC") as xCustomTable
            };
            Vars.structure_table = tableStructureCabel;
            Vars.structure_table_protocol = Vars.protocol.FindName("tableStructureCabel") as xCustomTable;

            Vars.AccessPath = Vars.settings.Access_Path;
            Vars.AccessTable = Vars.settings.Access_Table;

            Vars.chart = Chart;
            Vars.chart.AxisY_Divider = 0.1f;
            Vars.chart.AxisY_Maximum = Vars.settings.MaxI;

            Vars.chart.AddChart(new System.Drawing.PointF[0], new System.Drawing.Pen(System.Drawing.Color.Blue, 3));
            Vars.chart.AddChart(new System.Drawing.PointF[0], new System.Drawing.Pen(System.Drawing.Color.Red, 3));
            Vars.chart.AddChart(new System.Drawing.PointF[0], new System.Drawing.Pen(System.Drawing.Color.Green, 3));

            Vars.Test.ROhm.OnEvent += ROhm_OnEvent;
            Vars.Test.Hipot.OnEvent += Hipot_OnEvent;
            Vars.settings.ChangesMade += Settings_ChangesMade;

            Equipment_Connect();

            CreateMainMenu();
            CreateTestList();
            CreateContextMenu();
            CreateTestInfo();
            CreateTestTables();

            TestList.SetCurrentItem(0);
            
            _tmr_display_time = new Timer(DisplayTime);
        }
        private void MainWindow_Close()
        {
            if (_tmr_display_time != null)
            {
                _tmr_display_time.Change(Timeout.Infinite, Timeout.Infinite);
                _tmr_display_time.Dispose();
            }

            //_tmr_memory_clean.Change(Timeout.Infinite, Timeout.Infinite);
            //_tmr_memory_clean.Dispose();

            Vars.Test.Hipot.IsReading = false;
            Vars.Test.Hipot.Disconnect();
            Vars.Test.ROhm.Disconnect();
            Vars.Test.AdamTCP_Disconnect();
            //Vars.Test.ShutDownEverything();
            //Vars.Test = null;
            if (Vars.AdamTCP != null)
                Vars.AdamTCP.Dispose();

            MainFunctions.Log("Завершение программы.");
            MainFunctions.Log("********");

            this.Close();
            //Environment.Exit(0);
        }
        /* ************************************************************************************************ */       
        #endregion
        /* ************************************************************************************************ */

        /* ************************************************************************************************ */
        /* Обработка событий элементов управления Главным окном                                             */
        /* ************************************************************************************************ */
        #region ГЛАВНАЯ ФОРМА 
        /* ************************************************************************************************ */
        private void title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed) return;
            this.DragMove();
        }
        private void btnClose_Click(object sender, MouseEventArgs e)
        {
            MainWindow_Close();
        }
        private void btnMin_Click(object sender, MouseEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnMax_Click(object sender, MouseEventArgs e)
        {
            if (this.WindowState == WindowState.Normal) this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }
        /* ************************************************************************************************ */
        #endregion
        /* ************************************************************************************************ */

        /* ************************************************************************************************ */
        /* Обработка событий пользовательских элементов                                                     */
        /* ************************************************************************************************ */
        #region ГЛАВНОЕ МЕНЮ Создание и обработка событий
        /* ************************************************************************************************ */
        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow_Close();
        }
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }
        /* ************************************************************************************************ */
        #endregion

        #region ПРОТОКОЛ
        private void bdrProtocol_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if ((bdrProtocol.ActualWidth < 0) || (bdrProtocol.ActualHeight < 0)) return;
            //MainFunctions.DrawProtocol(ref imgProtocol, new System.Drawing.Size((int)bdrProtocol.ActualWidth, (int)bdrProtocol.ActualHeight));
        }
        #endregion

        #region СПИСОК ТЕСТОВ
        /* ************************************************************************************************ */
        private void TestList_CurrentItem_Changed(object sender, RoutedEventArgs e)
        {
            ReadRecord();
        }
        /* ************************************************************************************************ */
        #endregion

        #region ПЕРЕКЛЮЧАТЕЛИ
        /* ************************************************************************************************ */
        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SwitchUserInterfaceMode((UserInterfaceMode)Tab.SelectedIndex);
            //if (Vars.Test == null) return;

            //if (Tab.SelectedIndex == 2)
            //{
            //    if (checkHipot_Reading.IsChecked == true) { if (Vars.Test.Hipot.IsReading == false) Vars.Test.Hipot.IsReading = true; }
            //    else { if (Vars.Test.Hipot.IsReading == true) Vars.Test.Hipot.IsReading = false; }
            //}
            //else
            //{
            //    if (Vars.Test.ROhm.ActiveFase != CabelData.ROhmFase.None) Vars.Test.ROhm.ActiveFase = CabelData.ROhmFase.None;
            //    if (checkHipot_Reading.IsChecked == true) checkHipot_Reading.IsChecked = false;
            //}
        }
        private void radioSwitch_Clicked(object sender, RoutedEventArgs e)
        {
            xRadioBtn rb = sender as xRadioBtn;
            
            if (rb == null) return;
            if (rb.Name.Contains("Hipot")) Hipot_SwitchFase(rb.IsChecked ? "" : rb.Name.Replace("radioHipot_",""));
            if (rb.Name.Contains("ROhm")) ROhm_SwitchFase(rb.IsChecked ? "" : rb.Name.Replace("radioROhm_", ""));
            
        }
        private void radioManual_Click(object sender, RoutedEventArgs e)
        {
            xRadioBtn ck = sender as xRadioBtn;
            if (ck == null) return;

            if (ck.Name.Contains("ROhm")) SwitchROhmInterface(true);
            if (ck.Name.Contains("Hipot")) SwitchHipotInterface(true);

        }
        private void checkHipot_Reading_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Vars.Test == null) return;
                if (Vars.Test.Hipot == null) return;

                if (checkHipot_Reading.IsChecked == true)
                {
                    Vars.Test.Hipot.StartReading();//.IsReading = chkHipot_Reading.IsChecked == true;
                }
                else
                {
                    Vars.Test.Hipot.StopReading();
                    dfHipot_U.TextBox.Text = "";
                    dfHipot_I.TextBox.Text = "";
                    dfHipot_R.TextBox.Text = "";
                    dfHipot_I1000.TextBox.Text = "";
                    dfHipot_R1000.TextBox.Text = "";
                    dfHipot_Time.TextBox.Text = "";
                    txtMessage.Text = "Для начала испытания включите 'Связь' ->>";
                    statusHipot.Status = false;
                }
                Console.WriteLine((checkHipot_Reading.IsChecked == true ? "Включение" : "Отключение") + " связи с Di30R");

                //stkHipotButtons.Visibility = checkHipot_Reading.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
            }
            catch (Exception ex) { }
        }
        /* ************************************************************************************************ */
        #endregion

        #region НОВЫЙ ТЕСТ, ПУСК, СЛАЙДЕР ЗАДВИЖКИ и МОИ КНОПКИ ДЕЙСТВИЙ
        /* ************************************************************************************************ */
        private void NewTest_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTest();            
        }
        private void xButtonStateManage_Click(object sender, RoutedEventArgs e)
        {
            if (e.RoutedEvent.Name == "MouseUp")
            {
                var tb = (TextBlock)sender;
                SelectAction(tb.Name);
            }
        }    
        /* ************************************************************************************************ */
        /* ************************************************************************************************ */
        #endregion
        
        #region КОНТЕКСТНОЕ МЕНЮ СПИСКА ТЕСТОВ
        private void menu_AddRecord(object sender, RoutedEventArgs e)
        {
            AddRecord();
        }
        private void menu_UpdateRecord(object sender, RoutedEventArgs e)
        {
            UpdateRecord();
        }
        private void menu_DeleteRecord(object sender, RoutedEventArgs e)
        {
            DeleteRecord();   
        }
        private void menu_ViewReport(object sender, RoutedEventArgs e)
        {
            MainFunctions.PrintProtocol();
        }
        #endregion
        /* ************************************************************************************************ */

        /* ************************************************************************************************ */
        /* Функционал основного окна                                                                        */
        /* ************************************************************************************************ */
        #region Собственно..
        /* ************************************************************************************************ */
        #region Создание элементов
        private void CreateMainMenu()
        {
            Label lblMenu = new Label();
            lblMenu.FontFamily = new System.Windows.Media.FontFamily("Geniso");
            lblMenu.Width = 150;
            lblMenu.FontWeight = FontWeights.Bold;
            lblMenu.FontSize = 16;
            lblMenu.Content = "Главное меню";
            lblMenu.Height = 30;
            Button btnSettings = new Button();
            btnSettings.Width = 150;
            btnSettings.Content = "Настройки..";
            btnSettings.Click += btnSettings_Click;
            Button btnQuit = new Button();
            btnQuit.Width = 150;
            btnQuit.Content = "Выход";
            btnQuit.Click += btnQuit_Click;
            Separator separator0 = new Separator();
            Separator separator1 = new Separator();

            StackPanel result = new StackPanel();
            result.Orientation = Orientation.Vertical;
            result.Children.Add(lblMenu);
            result.Children.Add(btnSettings);
            result.Children.Add(separator1);
            result.Children.Add(btnQuit);

            wndTitle.SetContent(result);
        }
        private void CreateTestList()
        {
            TestList.CurrentItem_Changed += TestList_CurrentItem_Changed;
            TestList.NewTest_Clicked += NewTest_Click;
            TestList.CreateTable(Vars.ListRow_Headers, Vars.ListRow_Bindings, Vars.ListRow_Widths);
            MainFunctions.FillTestList(ref TestList);
        }
        private void CreateContextMenu()
        {
            TestList.cmUpdate_Click += menu_UpdateRecord;
            TestList.cmDelete_Click += menu_DeleteRecord;
            TestList.cmPrintReport_Click += menu_ViewReport;
        }
        private void CreateTestInfo()
        {
            frameProtocol.Content = Vars.protocol;

            stkDescription.Orientation = Orientation.Vertical;
            stkDescription.Margin = new Thickness(5);

            wrapDescription.Height = 30;
            wrapDescription.Orientation = Orientation.Vertical;
            wrapDescription.ClipToBounds = true;
            MainFunctions.CreateTestDescription(ref wrapDescription, Vars.DataField_Names, Vars.dfHeader_1, Vars.DataField_Types, Vars.DataField_IsEditable);

            grpDescription.Foreground = Brushes.Yellow;
            grpDescription.VerticalAlignment = VerticalAlignment.Top;
            grpDescription.HorizontalAlignment = HorizontalAlignment.Left;

            grpComment.Foreground = Brushes.Yellow;
            grpComment.VerticalAlignment = VerticalAlignment.Top;
            grpComment.HorizontalAlignment = HorizontalAlignment.Left;

            Comment.Width = 595;// 320 * 2;
            Comment.Height = 60;
            Comment.Margin = new Thickness(163, 0, 40, 0);
        }
        private void CreateTestTables()
        {
            // ROHM
            MainFunctions.CreateTable_ROhm(Vars.rohm_table);
            MainFunctions.CreateTable_ROhm(Vars.rohm_table_protocol);           

            // HIPOT
            MainFunctions.CreateTable_Hipot(Vars.hipot_tables[0], true,  Vars.TestTime_Short);
            MainFunctions.CreateTable_Hipot(Vars.hipot_tables[1], false, Vars.TestTime_Short);
            MainFunctions.CreateTable_Hipot(Vars.hipot_tables[2], false, Vars.TestTime_Short);

            MainFunctions.CreateTable_Hipot(Vars.hipot_tables_protocol[0], true,  Vars.TestTime_Short);
            MainFunctions.CreateTable_Hipot(Vars.hipot_tables_protocol[1], false, Vars.TestTime_Short);
            MainFunctions.CreateTable_Hipot(Vars.hipot_tables_protocol[2], false, Vars.TestTime_Short);

            // STRUCTURE CABEL
            MainFunctions.CreateTable_Struct(Vars.structure_table);
            MainFunctions.CreateTable_Struct(Vars.structure_table_protocol);

            // PI
            MainFunctions.CreateTable_PI(Vars.pi_table);
            MainFunctions.CreateTable_PI(Vars.pi_table_protocol);

            // настройка таблицы для отображении в протоколе - только рамки
            MainFunctions.SetProtocolCellStyle(Vars.rohm_table_protocol);
            MainFunctions.SetProtocolCellStyle(Vars.hipot_tables_protocol[0]);
            MainFunctions.SetProtocolCellStyle(Vars.hipot_tables_protocol[1]);
            MainFunctions.SetProtocolCellStyle(Vars.hipot_tables_protocol[2]);
            MainFunctions.SetProtocolCellStyle(Vars.pi_table_protocol);
            Vars.structure_table_protocol.SettingInProtocol_Cells(Brushes.Transparent, Brushes.Transparent, true, true, true, true, 7);
        }
        #endregion
        /* ************************************************************************************************ */
        private void ShowSettings()
        {
            MainFunctions.Log("Открытие настроек программы.");

            Window wnd = new Window();
            wnd.Owner = this;

            wnd.AllowsTransparency = true;
            wnd.SizeToContent = SizeToContent.WidthAndHeight;
            wnd.Content = Vars.settings;
            Vars.settings.owner = wnd;
            wnd.WindowStyle = System.Windows.WindowStyle.None;
            wnd.ShowDialog();

            wndTitle.HideMenu();
        }
        private void Settings_ChangesMade(object sender, RoutedEventArgs e)
        {
            MainFunctions.Log("Изменение настроек программы.");

            Equpment_Reconnect();
        }
        private void Equpment_Reconnect()
        {
            Equipment_Disconnect();
            Equipment_Connect();    
        }
        private void Equipment_Connect()
        {
            MainFunctions.Log("Подключение - БД...");
            statusDB.Status = AccessFuncs.Connect();
            MainFunctions.Log("--> " + (statusDB.Status ? "успех." : "ошибка."));
            MainFunctions.Log("Подключение - Adam5000TCP...");
            statusController.Status = Vars.Test.AdamTCP_Connect();
            MainFunctions.Log("--> " + (statusController.Status ? "успех." : "ошибка."));
            MainFunctions.Log("Подключение - CA6752...");
            statusCA6752.Status = Vars.Test.ROhm.Connect();
            MainFunctions.Log("--> " + (statusCA6752.Status ? "успех." : "ошибка."));
            MainFunctions.Log("Подключение - Di30R...");
            statusDi30R.Status = Vars.Test.Hipot.Init(Vars.settings.COM_Di30R);
            MainFunctions.Log("--> " + (statusDi30R.Status ? "успех." : "ошибка."));
        }
        private void Equipment_Disconnect()
        {
            AccessFuncs.Disconnect();
            Vars.Test.AdamTCP_Disconnect();
            Vars.Test.ROhm.Disconnect();
            Vars.Test.Hipot.Disconnect();
        }
        /* ************************************************************************************************ */
        private void SwitchUserInterfaceMode(UserInterfaceMode mode)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (TestList != null) TestList.IsEnabled = mode != UserInterfaceMode.Locked;

                //tabDescription.IsEnabled = mode != UserInterfaceMode.Locked;
                xDataField df;
                df = tabDescription.FindName("Length") as xDataField;
                if (df != null) df.IsEnabled = mode != UserInterfaceMode.Locked;
                df = tabDescription.FindName("CabelSort") as xDataField;
                if (df != null) df.IsEnabled = mode != UserInterfaceMode.Locked;

                tabProtocol.IsEnabled = mode != UserInterfaceMode.Locked;

                btnNewTest.IsEnabled = mode != UserInterfaceMode.Locked;

                SwitchROhmInterface(mode != UserInterfaceMode.Locked);
                SwitchHipotInterface(mode != UserInterfaceMode.Locked);

                Vars.Test.SwitchMayak(mode == UserInterfaceMode.Locked);
                
                wndTitle.HideMenu();
            }));
        }       
        private void SwitchROhmInterface(bool is_enabled)
        {
            bool state = is_enabled && (radioROhm_Manual.IsChecked == true);
            stkROhm_fases.IsEnabled = state;

            btnROhm_Start.IsEnabled = is_enabled;
            btnROhm_Save.IsEnabled = is_enabled;

            radioROhm_Manual.Visibility = is_enabled ? Visibility.Visible : Visibility.Hidden;
            
            //radioROhm_AB.Visibility = state ? Visibility.Visible : Visibility.Hidden;
            //radioROhm_BC.Visibility = state ? Visibility.Visible : Visibility.Hidden;
            //radioROhm_CA.Visibility = state ? Visibility.Visible : Visibility.Hidden;

            radioROhm_AB.IsEnabled = state;
            radioROhm_BC.IsEnabled = state;
            radioROhm_CA.IsEnabled = state;

            radioROhm_AB.Opacity = state ? 1 : 0.50;
            radioROhm_BC.Opacity = state ? 1 : 0.50;
            radioROhm_CA.Opacity = state ? 1 : 0.50;
        }
        private void SwitchHipotInterface(bool is_enabled)
        {
            bool state = is_enabled && (radioHipot_Manual.IsChecked == true);

            btnHipot_Start.IsEnabled = is_enabled;
            btnHipot_Save.IsEnabled = is_enabled;

            radioHipot_Manual.Visibility = is_enabled ? Visibility.Visible : Visibility.Hidden;
            checkHipot_Reading.Visibility = is_enabled ? Visibility.Visible : Visibility.Hidden;

            //radioHipot_A.Visibility = state ? Visibility.Visible : Visibility.Hidden;
            //radioHipot_B.Visibility = state ? Visibility.Visible : Visibility.Hidden;
            //radioHipot_C.Visibility = state ? Visibility.Visible : Visibility.Hidden;

            radioHipot_A.IsEnabled = state;
            radioHipot_B.IsEnabled = state;
            radioHipot_C.IsEnabled = state;

            radioHipot_A.Opacity = state ? 1 : 0.50;
            radioHipot_B.Opacity = state ? 1 : 0.50;
            radioHipot_C.Opacity = state ? 1 : 0.50;
        }
        /* ************************************************************************************************ */
        private void SelectAction(string button_name)
        {
            switch (button_name)
            {
                case "btnNewTest":
                    MainFunctions.Log("Создание нового теста.");
                    Action_NewTest();
                    break;
                case "btnPrint":
                    //case "btnPrint2":
                    MainFunctions.Log("Печать протокола.");
                    MainFunctions.PrintProtocol();
                    break;
                case "btnRecord_Save":
                    MainFunctions.Log("Сохранение результатов.");
                    UpdateRecord();
                    break;
                case "btnROhm_Start":
                    MainFunctions.Log("    ROhm - Старт измерения.");
                    ROhm_StartTesting();
                    break;
                case "btnROhm_Stop":
                    MainFunctions.Log("    ROhm - Остановка измерения.");
                    ROhm_StopTesting();
                    break;
                case "btnROhm_Save":
                    MainFunctions.Log("ROhm - Сохранение результатов.");
                    MainFunctions.ManualSave_ROhm();
                    UpdateRecord();
                    break;
                case "btnHipot_Start":
                    MainFunctions.Log("Hipot - Старт измерения.");
                    Action_StartHipot();
                    break;
                case "btnHipot_Stop":
                    MainFunctions.Log("Hipot - Остановка измерения.");
                    Hipot_StopTesting();
                    break;
                case "btnHipot_Save":
                    MainFunctions.Log("Hipot - Сохранение результатов.");
                    UpdateRecord();
                    break;
            }
        }
        private void Action_NewTest()
        {
            StackPanel stk = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            xDataField order = new xDataField() { Header = "Наряд/Заявка:"};
            xDataField serial = new xDataField() { Header = "Заводской номер:" };
            stk.Children.Add(order);
            stk.Children.Add(serial);
            stk.RenderTransform = new ScaleTransform(1.6, 1.6);
            stk.RenderTransformOrigin = new Point(0, 0);
            
            xMessageWindow msg = new xMessageWindow()
            {
                Width = 470,
                Height = 250,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = "Новый тест",
                Content = stk
            };
            
            if (msg.Show() != MessageBoxResult.Yes) return;
            if ((order.Value == "") || (serial.Value == "")) return;

            Vars.CurrentCabel = new CabelData()
            {
                DateTime = DateTime.Now,
                Order = order.Value,
                Serial = serial.Value
            };
            if(AccessFuncs.AddRecord()) MainFunctions.Log("Добавлена новая запись."); ;
            MainFunctions.FillTestList(ref TestList);
            TestList.SetCurrentItem(0);
            Tab.SelectedIndex = 1;
        }
        private void Action_StartHipot()
        {
            if (!MainFunctions.Check_HipotDoorClosed())
            {
                MainFunctions.Log("Hipot - остановка. Калитка не закрыта.");
                return;
            }

            if (!MainFunctions.SaveCurrentCabel_Data(wrapDescription, Comment.Text))
            {
                MainFunctions.Log("Hipot - остановка. Не удалось произвести предварительное сохранение перед испытанием.");
                return;
            }
                
            if (!Vars.CurrentCabel.CheckNecessaryFields())
            {
                Tab.SelectedIndex = (int)UserInterfaceMode.TestDesc;
                MainFunctions.Log("Hipot - остановка. Не заданы необходимые значения.");
                return;
            }

            xMessageWindow msg = new xMessageWindow()
            {
                Title = "Параметры теста",
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new Frame() { Content = Vars.hipot_params },
                Width = 500,
                Height = 200
            };

            if (msg.Show() != MessageBoxResult.Yes)
            {
                MainFunctions.Log("Hipot - отмена старта измерения.");
                return;
            }

            if (!MainFunctions.Check_HipotVoltage()) return;


            if (radioHipot_Manual.IsChecked == true) Hipot_StartManual();
            else Hipot_StartAuto();
        }
        private void CreateNewTest()
        {
            Comment.Text = "";
            tbCurrentID.Text = "";

            SwitchUserInterfaceMode(UserInterfaceMode.TestDesc);
            Tab.SelectedIndex = (int)UserInterfaceMode.TestDesc;
        }
        /* ************************************************************************************************ */
        #region Сопротивленеи изоляции HIPOT
        private void Hipot_SetDefaultState()
        {
            if (Vars.Test.Hipot.IsConnected) Vars.Test.Hipot.Shutdown();
            Hipot_SwitchFase("");

            MainFunctions.Log("Hipot - выключение реле.");
        }
        private void Hipot_StartManual()
        {
            if (!MainFunctions.Check_HipotFase()) return;

            int testing_time = Vars.hipot_params.IsWithPolar ? Vars.TestTime_Long : Vars.TestTime_Short;

            MainFunctions.CreateTable_Hipot(Vars.hipot_tables[(int)Vars.Test.Hipot.ActiveFase],
                                            Vars.Test.Hipot.ActiveFase == CabelData.HipotFase.A,
                                            testing_time);
            MainFunctions.CreateTable_Hipot(Vars.hipot_tables_protocol[(int)Vars.Test.Hipot.ActiveFase],
                                            Vars.Test.Hipot.ActiveFase == CabelData.HipotFase.A,
                                            testing_time);

            Vars.CurrentCabel.Hipot_RemovePoints(Vars.Test.Hipot.ActiveFase);
            Vars.chart.GetChart((int)Vars.Test.Hipot.ActiveFase).RemovePoints();
            Vars.chart.FrontChartIndex = (int)Vars.Test.Hipot.ActiveFase;
            Vars.chart.AxisX_Maximum = testing_time;
            Vars.chart.AxisX_Divisions = testing_time;
            
            SwitchUserInterfaceMode(UserInterfaceMode.Locked);
            _is_Hipot_ended = false;
            Vars.Test.Hipot.IsWithPolar = Vars.hipot_params.IsWithPolar;

            MainFunctions.Log("Hipot - старт в ручном режиме: фаза " + Vars.Test.Hipot.ActiveFase.ToString());
            new Thread(Hipot_ManualTesting_Thread).Start();
        }
        private void Hipot_StartAuto()
        {
            int testing_time = Vars.hipot_params.IsWithPolar ? Vars.TestTime_Long : Vars.TestTime_Short;
            for (int i = 0; i < 3; i++)
            {
                MainFunctions.CreateTable_Hipot(Vars.hipot_tables[i], i == 0, testing_time);
                MainFunctions.CreateTable_Hipot(Vars.hipot_tables_protocol[i], i == 0, testing_time);
            }

            Vars.CurrentCabel.Hipot_RemovePoints(CabelData.HipotFase.A);
            Vars.CurrentCabel.Hipot_RemovePoints(CabelData.HipotFase.B);
            Vars.CurrentCabel.Hipot_RemovePoints(CabelData.HipotFase.C);
            Vars.chart.GetChart((int)CabelData.HipotFase.A).RemovePoints();
            Vars.chart.GetChart((int)CabelData.HipotFase.B).RemovePoints();
            Vars.chart.GetChart((int)CabelData.HipotFase.C).RemovePoints();
            Vars.chart.AxisX_Maximum = testing_time;
            Vars.chart.AxisX_Divisions = testing_time;

            SwitchUserInterfaceMode(UserInterfaceMode.Locked);
            _is_Hipot_ended = false;
            Vars.Test.Hipot.IsWithPolar = Vars.hipot_params.IsWithPolar;

            MainFunctions.Log("Hipot - старт в автоматическом режиме.");
            new Thread(Hipot_AutoTesting_Thread).Start();
        }
        private void Hipot_StopTesting()
        {
            try
            {
                Vars.Test.Hipot.Stop();

                _tmr_display_time.Change(Timeout.Infinite, Timeout.Infinite);
                statusHipot.Status = false;
                Vars.Test.IsHipotInterrupted = true;

                MainFunctions.Log("Hipot - прерывание испытания.");
            }
            catch (Exception ex) { }
        }
        private void Hipot_SwitchFase(string fase_name)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (fase_name)
                {
                    case "A":
                        radioHipot_A.IsChecked = true;
                        radioHipot_B.IsChecked = false;
                        radioHipot_C.IsChecked = false;
                        Vars.Test.Hipot.ActiveFase = CabelData.HipotFase.A;                       
                        break;
                    case "B":
                        radioHipot_A.IsChecked = false;
                        radioHipot_B.IsChecked = true;
                        radioHipot_C.IsChecked = false;
                        Vars.Test.Hipot.ActiveFase = CabelData.HipotFase.B;
                        break;
                    case "C":
                        radioHipot_A.IsChecked = false;
                        radioHipot_B.IsChecked = false;
                        radioHipot_C.IsChecked = true;
                        Vars.Test.Hipot.ActiveFase = CabelData.HipotFase.C;
                        break;
                    default:
                        radioHipot_A.IsChecked = false;
                        radioHipot_B.IsChecked = false;
                        radioHipot_C.IsChecked = false;
                        Vars.Test.Hipot.ActiveFase = CabelData.HipotFase.None;
                    break;
                }
                MainFunctions.Log("Hipot - переключение фаз: " + Vars.Test.Hipot.ActiveFase.ToString());
            }));
        }
        private void Hipot_AutoTesting_Thread(object obj)
        {
            Hipot_SwitchFase("A");
            Thread.Sleep(2000);
            Hipot_TestActiveFase();
            Thread.Sleep(5000);

            if (Hipot_Check_Interrupted()) return;
            
            Hipot_SwitchFase("B");
            Thread.Sleep(2000);
            Hipot_TestActiveFase();
            Thread.Sleep(5000);

            if (Hipot_Check_Interrupted()) return;
            
            Hipot_SwitchFase("C");
            Thread.Sleep(2000);
            Hipot_TestActiveFase();
            Thread.Sleep(5000);

            Dispatcher.BeginInvoke(new Action(() =>
            { Hipot_EndTesting(); }));
            Thread.Sleep(5000);
        }
        private void Hipot_ManualTesting_Thread(object obj)
        {
            Hipot_TestActiveFase();

            Dispatcher.BeginInvoke(new Action(() =>
            { Hipot_EndTesting(); }));
            Thread.Sleep(5000);
        }
        private void Hipot_TestActiveFase()
        {
            Vars.chart.FrontChartIndex = (int)Vars.Test.Hipot.ActiveFase;

            Vars.Test.Hipot_Start();
            Thread.Sleep(1000);
            bool _door_is_open_alarmed = false;
            while (!Vars.Test.Hipot.IsEnded) {
                if ((!_door_is_open_alarmed) && (!MainFunctions.Check_HipotDoorClosed()))
                {
                    _door_is_open_alarmed = true;
                    Vars.Test.IsHipotInterrupted = true;
                    SelectAction("btnHipot_Stop");
                    return;
                }
                Thread.Sleep(100);
            }
            Thread.Sleep(2000);
            Hipot_SwitchFase("");
            Thread.Sleep(1000);
        }
        private void Hipot_EndTesting()
        {
            SwitchUserInterfaceMode(UserInterfaceMode.TestView);
            _tmr_display_time.Change(Timeout.Infinite, Timeout.Infinite);
            statusHipot.Status = false;
        }
        private bool Hipot_Check_Interrupted()
        {
            if (!Vars.Test.IsHipotInterrupted) return false;

            Hipot_SwitchFase("");
            SwitchUserInterfaceMode(UserInterfaceMode.TestView);
            Vars.Test.IsHipotInterrupted = false;

            return true;
        }
        private void Hipot_Time(bool start)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (start)
                {
                    _test_start_time = new DateTime();
                    _tmr_display_time.Change(0, 1000);
                }
                else
                {
                    _test_start_time = new DateTime();
                    _tmr_display_time.Change(Timeout.Infinite, Timeout.Infinite);
                }
                dfHipot_Time.Value = _test_start_time.ToString("mm:ss");
            }));
        }
        private void Hipot_OnEvent(object sender, xEquipment.xDi30R.Di30R_EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            { Hipot_ProcessRecievedData(e); }));
        }
        private void Hipot_ProcessRecievedData(xEquipment.xDi30R.Di30R_EventArgs args)
        {
            if (args == null) return;
            
            if (args.Message == "UpdateStatesInfo")
            {
                radioHipot_A.IsChecked = args.Fases[0];
                radioHipot_B.IsChecked = args.Fases[1];
                radioHipot_C.IsChecked = args.Fases[2];
            }
            else
            {
                if (checkHipot_Reading.IsChecked != true) return;

                float value_u = args.Voltage;
                float value_i = args.CurrentHi;
                float value_r = Vars.CurrentCabel.Hipot_CalculateRiz(value_u, value_i, false);
                float value_i1000 = value_i * Vars.CurrentCabel.LengthCoeff;
                float value_r1000 = value_r * Vars.CurrentCabel.LengthCoeff;

                dfHipot_U.Value = value_u.ToString("0.0000");
                dfHipot_I.Value = value_i.ToString("0.0000");
                dfHipot_R.Value = value_r.ToString("0.00");

                dfHipot_I1000.Value = value_i1000.ToString("0.0000");
                dfHipot_R1000.Value = value_r1000.ToString("0.00");

                int secs = _test_start_time.Second + _test_start_time.Minute * 60;
                if (secs > Vars.Test.Hipot.TestingTime + 61) secs = 0;
                if (secs < 60) secs = 59;
                secs -= 60;

                Vars.Test.Hipot_MoveMarker(secs, value_i1000 / Vars.chart.AxisY_Divider);

                switch(args.Message)
                {
                    case "ResetTime":
                        Hipot_Time(true);
                        break;
                    case "FixPoint":
                        if (value_i > 0)
                        {
                            if (secs < 0) break;
                            try
                            {
                                Vars.chart.GetChart((int)Vars.Test.Hipot.ActiveFase).AddPoint(Vars.CurrentCabel.CreatePoint(secs, value_i / Vars.chart.AxisY_Divider));
                                Vars.chart.Refresh();
                            }
                            finally { }
                        }
                        break;
                    case "TestStopped":
                        Hipot_Time(false);
                        break;
                    case "SafeVoltage":

                        break;
                }
                txtMessage.Text = args.Message;
                txtHIGHVOLTAGE.Visibility = args.Voltage > xEquipment.xDi30R.HIGH_VOLTAGE_ALARM ? Visibility.Visible : Visibility.Hidden;
            }

            //txtMessage.Text = args.Message;
            statusHipot.Status = !statusHipot.Status && checkHipot_Reading.IsChecked == true;
        }
        #endregion
        /* ************************************************************************************************ */
        #region Омическое сопротивление
        private void ROhm_SetDefaultState()
        {
            Vars.Test.ROhm.ActiveFase = CabelData.ROhmFase.None;

            MainFunctions.Log("    ROhm. - выключение реле.");
        }
        private void ROhm_StartTesting()
        {
            if (radioROhm_Manual.IsChecked == true)
                if (Vars.Test.ROhm.ActiveFase == CabelData.ROhmFase.None)
                {
                    MainFunctions.ShowMessage("ROhm", "Выберите фазу для испытания.", false);
                    MainFunctions.Log("    ROhm - остановка. Не выбрана активная фаза.");

                    return;
                }

            if (!MainFunctions.SaveCurrentCabel_Data(wrapDescription, Comment.Text)) return;
            if (!Vars.CurrentCabel.CheckNecessaryFields())
            {
                Tab.SelectedIndex = (int)UserInterfaceMode.TestDesc;
                return;
            }

            SwitchUserInterfaceMode(UserInterfaceMode.Locked);

            Vars.CurrentCabel.ROhm_FillTable(ref tableROhm, true);

            if (radioROhm_Manual.IsChecked == true) {
                new Thread(ROhm_ManualTesting_Thread).Start();
                MainFunctions.Log("    ROhm - Запуск испытания в ручном режиме.");
            }
            else {
                new Thread(ROhm_AutoTesting_Thread).Start();
                MainFunctions.Log("    ROhm - Запуск испытания в автоматическом режиме.");
            }
        }
        private void ROhm_StopTesting()
        {
            if (!Vars.Test.ROhm.IsActive) return;
            
            Vars.Test.ROhm.Abort();
            Vars.Test.IsROhmInterrupted = true;
            MainFunctions.Log("    ROhm - Остановка испытания.");
        }
        private void ROhm_SwitchFase(string fase_name)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (fase_name)
                {
                    case "AB":
                        radioROhm_AB.IsChecked = true;
                        radioROhm_BC.IsChecked = false;
                        radioROhm_CA.IsChecked = false;
                        Vars.Test.ROhm.ActiveFase = CabelData.ROhmFase.AB;
                        MainFunctions.Log("    ROhm - Переключение фаз АВ.");
                        break;
                    case "BC":
                        radioROhm_AB.IsChecked = false;
                        radioROhm_BC.IsChecked = true;
                        radioROhm_CA.IsChecked = false;
                        Vars.Test.ROhm.ActiveFase = CabelData.ROhmFase.BC;
                        MainFunctions.Log("    ROhm - Переключение фаз ВС.");
                        break;
                    case "CA":
                        radioROhm_AB.IsChecked = false;
                        radioROhm_BC.IsChecked = false;
                        radioROhm_CA.IsChecked = true;
                        Vars.Test.ROhm.ActiveFase = CabelData.ROhmFase.CA;
                        MainFunctions.Log("    ROhm - Переключение фаз СА.");
                        break;
                    default:
                        radioROhm_AB.IsChecked = false;
                        radioROhm_BC.IsChecked = false;
                        radioROhm_CA.IsChecked = false;
                        Vars.Test.ROhm.ActiveFase = CabelData.ROhmFase.None;
                        MainFunctions.Log("    ROhm - Выключение фаз.");
                        break;
                }
            }));
        }
        private void ROhm_AutoTesting_Thread(object obj)
        {
            ROhm_SwitchFase("AB");
            Thread.Sleep(1000);
            ROhm_TestActiveFase();

            if (ROhm_Check_Interrupted()) return;

            ROhm_SwitchFase("BC");
            Thread.Sleep(1000);
            ROhm_TestActiveFase();

            if (ROhm_Check_Interrupted()) return;

            ROhm_SwitchFase("CA");
            Thread.Sleep(1000);
            ROhm_TestActiveFase();

            ROhm_SwitchFase("");
            SwitchUserInterfaceMode(UserInterfaceMode.TestView);

            Dispatcher.BeginInvoke(new Action(() =>
            { Vars.CurrentCabel.ROhm_FillTable(ref tableROhm); }));
        }
        private void ROhm_ManualTesting_Thread(object obj)
        {
            ROhm_TestActiveFase();

            ROhm_SwitchFase("");
            SwitchUserInterfaceMode(UserInterfaceMode.TestView);

            Dispatcher.BeginInvoke(new Action(() =>
            { Vars.CurrentCabel.ROhm_FillTable(ref tableROhm); }));
        }
        private void ROhm_TestActiveFase()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            { MainFunctions.Log("    ROhm - Испытание текущих фаз..."); }));

            Vars.Test.ROhm.TestFase();
            while (Vars.Test.ROhm.IsActive) { Thread.Sleep(1); }
        }
        private bool ROhm_Check_Interrupted()
        {
            if (!Vars.Test.IsROhmInterrupted) return false;

            Dispatcher.BeginInvoke(new Action(() =>
            { MainFunctions.Log("    ROhm - Прерывание испытания..."); }));

            ROhm_SwitchFase("");
            SwitchUserInterfaceMode(UserInterfaceMode.TestView);
            Vars.Test.IsROhmInterrupted = false;

            return true;
        }
        private void ROhm_OnEvent(object sender, xEquipment.xCA6250.CA6250_EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            { ROhm_ProcessReceivedData(e); }));
        }
        private void ROhm_ProcessReceivedData(xEquipment.xCA6250.CA6250_EventArgs args)
        { 
            MainFunctions.Log("    ROhm - Событие: Message --> " + args.Message);
            MainFunctions.Log("    ROhm - Событие: Value --> " + args.Value);

            bool success = args.Message.Contains("Success");
            if (success) Vars.CurrentCabel.ROhm_AddValue(args.Value, Vars.Test.ROhm.ActiveFase);
            else
            {
                MainFunctions.Log("    ROhm " + Vars.Test.ROhm.ActiveFase.ToString() + ": " + args.Message);
                ROhm_StopTesting();
            }
            tableROhm.SetCellValue(((int)Vars.Test.ROhm.ActiveFase + 1), 3, success ? args.Value.ToString(): args.Message);
        }
        #endregion
        /* ************************************************************************************************ */
        private void AddRecord()
        {
            if (!MainFunctions.SaveCurrentCabel_Data(wrapDescription, Comment.Text)) return;
            if (AccessFuncs.AddRecord())
            {
                MainFunctions.FillTestList(ref TestList);
                tbCurrentID.Text = Vars.CurrentCabel.ID.ToString();
                MainFunctions.Log("Добавление записи № " + Vars.CurrentListRow.RecID.ToString());
            }
        }
        private void ReadRecord()
        {
            // Получаем ID строки для БД из выбранного строки списка тестов
            Vars.CurrentListRow = (Vars.TestListRow)TestList.CurrentItem;

            // Создаём экземпляр класса Информации об испытании
            // и заполняем его из соответствующей строки БД
            Vars.CurrentCabel = new CabelData(Vars.CurrentListRow.RecID);

            // Заполняем поля на странице описания
            MainFunctions.FillDataFields_CurrentValues(ref wrapDescription);
            Comment.Text = Vars.CurrentCabel.GetItemValue("Comment");
            (Vars.protocol.FindName("txtComment") as TextBox).Text = Comment.Text;

            tbCurrentID.Text = Vars.CurrentCabel.ID.ToString();
            // Заполняем поля результатов измерения омического сопротивления          
            Vars.CurrentCabel.ROhm_FillTable(ref tableROhm);
            xCustomTable temp = Vars.protocol.FindName("tableROhm") as xCustomTable;
            Vars.CurrentCabel.ROhm_FillTable(ref temp);

            // Заполняем таблицу релультатов испытания и строим график 
            Vars.CurrentCabel.Hipot_FillTable(CabelData.HipotFase.A);
            Vars.CurrentCabel.Hipot_FillTable(CabelData.HipotFase.B);
            Vars.CurrentCabel.Hipot_FillTable(CabelData.HipotFase.C);
            Vars.CurrentCabel.BuildGraphics();
            // заполнение данных о индексе поляризации
            //Vars.CurrentCabel.FillTable_PI(ref tablePI);
            //xCustomTable table_PI = Vars.protocol.FindName("tablePI") as xCustomTable;
            //Vars.CurrentCabel.FillTable_PI(ref table_PI);

            // заполнение таблицы "Информация о структуре кабеля"
            Vars.CurrentCabel.StructureCabel_FillTable(ref Vars.structure_table);
            Vars.CurrentCabel.StructureCabel_FillTable(ref Vars.structure_table_protocol);
            Vars.protocol.ShowStructureData(true/*Vars.CurrentCabel.IsStructureDataPresent*/);

            
            MainFunctions.Log("Чтение записи № " + Vars.CurrentListRow.RecID.ToString());

            radioHipot_Manual.IsChecked = false;
            
            Vars.protocol.SetChart(Vars.chart.DrawToBitmap(Vars.protocol_chart_width, Vars.protocol_chart_height));
            //if ((bdrProtocol.ActualWidth < 0) || (bdrProtocol.ActualHeight < 0)) return;
            //MainFunctions.DrawProtocol(ref imgProtocol, new System.Drawing.Size((int)bdrProtocol.ActualWidth, (int)bdrProtocol.ActualHeight));
        }
        private void UpdateRecord()
        {
            if (!MainFunctions.SaveCurrentCabel_Data(wrapDescription, Comment.Text)) return;

            if (AccessFuncs.UpdateRecord())
            {
                //!!!!!!! В случае тормозов
                //Vars.CurrentCabel.Hipot_FillTable(CabelData.HipotFase.A);
                //Vars.CurrentCabel.Hipot_FillTable(CabelData.HipotFase.B);
                //Vars.CurrentCabel.Hipot_FillTable(CabelData.HipotFase.C);
                ReadRecord();
                
                
                MainFunctions.Log("Обновление записи № " + Vars.CurrentListRow.RecID.ToString());
            }
        }
        private void DeleteRecord()
        {
            MainFunctions.Log("Удаление записи № " + Vars.CurrentListRow.RecID.ToString());
            AccessFuncs.DeleteRecord(ref TestList);
        }
        /* ************************************************************************************************ */
        /* ************************************************************************************************ */
        #endregion
        /* ************************************************************************************************ */

        /* ************************************************************************************************ */
        /* Всякая хрень, дребедень и лабудень                                                               */
        /* ************************************************************************************************ */
        private void DisplayTime(object obj)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _test_start_time = _test_start_time.AddSeconds(1);
                if (Vars.Test.CurrentTestMode == CabelTest.TestMode.Hipot)
                {
                    dfHipot_Time.Value = _test_start_time.ToString("mm:ss");
                    if (_test_start_time.Second == 0)
                        try
                        {
                            float value_u;
                            float value_i;
                            if (float.TryParse(dfHipot_U.Value, out value_u) && float.TryParse(dfHipot_I.Value, out value_i))
                                Vars.CurrentCabel.Hipot_AddPoint(Vars.Test.Hipot.ActiveFase, (_test_start_time.Minute - 1) * 60, value_u, value_i);
                        }
                        catch (Exception ex) { }
                }
            }));
        }
        private void MemoryCleaner(object obj)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        /// <summary>
        /// ТУТ Я ПРОБУЮ ВСЯКУЮ ДРЕБЕДЕНЬ
        /// </summary>
        private void tbLog_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    string[] str = tbLog.Text.Split(';');
            //    float[] dbl = new float[str.Length];
            //    for (int i = 0; i < dbl.Length; i++ )
            //    {
            //        dbl[i] = float.Parse(str[i]);
            //    }
            //    float stages = (float)Vars.CurrentCabel.nStages;
            //    ValueTable.SetValue("Flow", dbl[0].ToString());
            //    ValueTable.SetValue("Lift", (dbl[1] * stages).ToString());
            //    ValueTable.SetValue("Power", (dbl[2] * stages).ToString());
            //    ValueTable.SetValue("Lift_stage", dbl[1].ToString());
            //    ValueTable.SetValue("Power_stage", dbl[2].ToString());
            //}
        }
        class BooleanToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is Boolean && (bool)value) return Visibility.Visible;
                return Visibility.Hidden;
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is Visibility && (Visibility)value == Visibility.Visible) return true;
                return false;
            }
        }


    }
}
