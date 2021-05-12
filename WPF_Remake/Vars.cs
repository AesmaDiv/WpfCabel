using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Controls;
using xLibrary;


namespace WPF_Try
{
    public class Vars
    {
        public static MainWindow mainWindow;

        public static int TestTime_Short = 5;
        public static int TestTime_Long = 10;

        public static string path_HomeDir = System.IO.Directory.GetCurrentDirectory();
        public static string path_Di30RCFG = path_HomeDir + "\\Di30R.cfg";
        public static string path_AdamCFG = path_HomeDir + "\\Adam.cfg";
        public static string path_TemplateDir = path_HomeDir + "\\Template\\";
        public static string path_Protocol = path_TemplateDir + "ProtocolTemplate.xlsm";
        public static string path_PumpChart = path_TemplateDir + "PumpChart.png";
        public static string path_Sortament = path_HomeDir + "\\CabelType\\Sortament.lst";

        public static System.Threading.Timer temp_timer;
        public static Journal journal = new Journal();

        public static xEquipment.xAdam5000TCP AdamTCP = new xEquipment.xAdam5000TCP();
        public static string[] AdamChannelNames = new string[] { "ROhm_FaseA", "ROhm_FaseB", "ROhm_FaseC", "CA6250_Print", "CA6250_Start", "Mayak", "Door" };
        public static xEquipment.xAdam5000TCP.Settings AdamSettings = new xEquipment.xAdam5000TCP.Settings()
        {
            IP = "10.0.0.1",
            Infos = new xEquipment.xAdam5000TCP.Settings.Info[]
            {
                new xEquipment.xAdam5000TCP.Settings.Info() { Name = "ROhm_FaseA", Slot = 0, Channel = 0 },
                new xEquipment.xAdam5000TCP.Settings.Info() { Name = "ROhm_FaseB", Slot = 0, Channel = 1 },
                new xEquipment.xAdam5000TCP.Settings.Info() { Name = "ROhm_FaseC", Slot = 0, Channel = 2 },
                new xEquipment.xAdam5000TCP.Settings.Info() { Name = "CA6250_Start", Slot = 0, Channel = 3 },
                new xEquipment.xAdam5000TCP.Settings.Info() { Name = "CA6250_Print", Slot = 0, Channel = 4 },
                new xEquipment.xAdam5000TCP.Settings.Info() { Name = "Mayak", Slot = 0, Channel = 5 },
                new xEquipment.xAdam5000TCP.Settings.Info() { Name = "Door", Slot = 0, Channel = 7 },
            }
        };

        // Переменные описания параметров работы с БД
        public static xAccess Access;
        public static string AccessPath = "";
        public static string AccessTable = "";
        public static string AccessColumnNames = "RecID,DateTime,Order,Producer,Serial,Customer,Field,Lease,Well," +
                                                 "CabelType,CabelSort,Length,DaysRun,Line,Extention,Temperature,Comment," +
                                                 "Test_ROhm";
        public static string[] AccessRawDataNames = new string[] { "Hipot_A", "Hipot_B", "Hipot_C" };

        public static string[] dfNames_protocol = new String[]     // Имена столбцов таблицы БД для протокола
        {
            "RecId","DateTime","Order",
            "Producer","Serial","Customer",
            "Field","Lease","Well",
            "CabelType","CabelSort",
            "Length","DaysRun","Line","Extention",
            "Temperature","Comment"
        };

        // Расчитываются
        public static string[] ProtocolFields_Calc = new string[]
        {

        };

        public static SettingPage settings = new SettingPage(@".\Settings.cfg");
        public static xSimpleChart chart;
        public static ProtocolPage protocol = new ProtocolPage();
        public static int protocol_chart_width = 724;
        public static int protocol_chart_height = 200;
        public static HipotParamsPage hipot_params = new HipotParamsPage();
        public static CabelTest Test = new CabelTest();
        
        public static int ROhm_stabilization_time = 10000;
        public static int ROhm_before_print_time = 3000;
        public static int ROhm_after_print_time = 5000;
        public static int ROhm_contact_short_time = 500;
        public static CabelData CurrentCabel = new CabelData();
        //public static xExcel protocol_print = new xLibrary.xExcel();

        public static xCustomTable rohm_table = new xCustomTable();
        public static xCustomTable rohm_table_protocol = new xCustomTable();
        public static xCustomTable[] hipot_tables = new xCustomTable[3];
        public static xCustomTable[] hipot_tables_protocol = new xCustomTable[3];
        public static xCustomTable pi_table = new xCustomTable();
        public static xCustomTable pi_table_protocol = new xCustomTable();
        public static xCustomTable structure_table = new xCustomTable();
        public static xCustomTable structure_table_protocol = new xCustomTable();

        public class TestTableRow
        {
            public int Num { get; set; }
            public string I_A { get; set; }
            public string U_A { get; set; }
            public string Riz_A { get; set; }
            public string I_B { get; set; }
            public string U_B { get; set; }
            public string Riz_B { get; set; }
            public string I_C { get; set; }
            public string U_C { get; set; }
            public string Riz_C { get; set; }

        }
        public class TestListRow
        {
            public int RecID { get; set; }
            public string DateTime { get; set; }
            public string Order { get; set; }
            public string Serial { get; set; }
            public string Extention { get; set; }
        }
        public static TestListRow CurrentListRow = new TestListRow();
        public static string[] ListRow_Headers = new String[] { "ID", "Дата", "Наряд-Заказ", "Завод.№", "Удлин.НЗ" };       // Имена столбцов списка тестов
        public static string[] ListRow_Bindings = new String[] { "RecID", "DateTime", "Order", "Serial", "Extention" };   // Имена столбцов БД для привязки
        public static double[] ListRow_Widths = new double[] { 60, 140, 145, 75, 80 };   // Имена столбцов БД для привязки

        #region Массивы панели описания Объекта испытания
        public static string[] dfHeader_1 = new String[]    // Имена полей описания
        {
            "Дата испытания","Наряд №/Заявка №","Заказчик","",
            "Месторождение","Куст","Скважина","",
            "Заводской номер","Производитель","Тип Кабеля","Сортамент",
            "Длина(м)","Суточный пробег","Кабельная линия","Удлинитель НЗ №"
        };
        public static string[] DataField_Names = new String[]     // Имена столбцов таблицы БД для привязки
        {
            "DateTime","Order","Customer","",
            "Field","Lease","Well","",
            "Serial","Producer","CabelType","CabelSort",
            "Length","DaysRun","Line","Extention"
        };
        public static bool[] DataField_Types = new bool[]         // Тип поля: true - СomboBox, false - TextBox
        {
            false,false,true,false,
            false,false,false,false,
            false,true,true,true,
            false,false,false,false
        };
        public static bool[] DataField_IsEditable = new bool[]    // Доступ на редактирование
        {
            false,false,true,true,
            true,true,true,true,
            false,true,true,false,
            true,true,true,true
        };
        public static string[] NecessaryFields = new string[] { "CabelType", "CabelSort", "Length" };
        #endregion

        #region Массивы для таблицы результатов

        public static string[] ROhmTable_ColHeaders = new string[]
        {
            "R, Ом",
            "R*, Ом",
            "Отклонение Ri, %",
            "Rмф, Ом"
        };

        public static string[] ROhmTable_RowHeaders = new string[]
        {
            "Ri (идеал.)",
            "Фаза А",
            "Фаза В",
            "Фаза С"
        };

        public static string[] HipotTable_Headers = new String[]
        {
            "U,кВ",
            "I,мкА",
            "I*,мкА",
            "Riz*,МОм",
        };

        public static string[] TestTable_Bindings = new String[]
        {
            "U_A",
            "I_A",
            "I_A1000",
            "Riz_A",
            "",
            "U_B",
            "I_B",
            "I_B1000",
            "Riz_B",
            "",
            "U_C",
            "I_C",
            "I_C1000",
            "Riz_C"
        };

        #region ЗАГОЛОВКИ ТАБЛИЦЫ СТРУКТУРЫ КАБЕЛЬНОЙ ЛИНИИ

        public static string[] StructCabelTable_Headers_Rows = new String[]
        {
            "Тер.Вст.",
            "Вст.1",
            "Вст.2",
            "Вст.3",
            "Вст.4",
            "Вст.5",
            "",
            "Удл."
        };

        public static string[] StructCabelTable_Headers_Columns = new String[]
        {
            "Завод",
            "Тип кабеля",
            "ø S, мм²",
            "L, м",
            "Год",
            "№ куска",
            "Нов./Рем."
        };

        public static string[] StructCabelTable_FloatingHeaders_Columns = new String[]
        {
            "Завод",
            "Тип кабеля",
            "ø S, мм²",
            "L, м",
            "Муфта",
            "№ удл.",
            "Нов./Рем."
        };

        #endregion

        #region ЗАГОЛОВКИ ТАБЛИЦЫ КАЧЕСТВА ИЗОЛЯЦИИ

        public static string[] PITable_Headers_Rows = new String[]
        {
            "Фаза А",
            "Фаза В",
            "Фаза С"
        };

        public static string[] PITable_Headers_Columns = new String[]
        {
            "PI (индекс поляризации)",
            "Качество изоляции"
        };

        #endregion

        #endregion
    }
}
