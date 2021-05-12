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
using System.IO;
using System.Globalization;
using System.Xml.Serialization;

namespace WPF_Try
{
    /// <summary>
    /// Логика взаимодействия для SettingPage.xaml
    /// </summary>
    public partial class SettingPage : Page
    {
        public Window owner;
        private Thickness default_marginSave, pressed_marginSave, default_marginClose, pressed_marginClose;
        private string _settingsFile = "";
        private bool _settingsChanged = false;
        private float[] _control_volts = new float[2];
        private float[] _coefs_voltage = new float[2];
        private float[] _coefs_current_hi = new float[2];
        private float[] _coefs_current_lo = new float[2];
        private float _maxU, _maxI;
        private int _stabilization_time;
        
        private bool SettingsChanged
        {
            get { return _settingsChanged; }
            set
            {
                _settingsChanged = value;
                if (_settingsChanged) bdrSave.BorderBrush = Brushes.Yellow;
                else bdrSave.BorderBrush = Brushes.White;
            }
        }

        
        public class ChannelParams
        {
            public string Header = "";
            public string Name = "";
            public int Slot = -1;
            public int Channel = -1;
            public int Offset = -1;
            public float Range = -1;
            public int MaxDigital = -1;
            public float Coeff = -1;
        }
        private Settings _settings;
        [Serializable]
        public class Settings
        {
            public float[] ControlVolts = new float[2];
            public float[] CoefsVoltage = new float[2];
            public float[] CoefsCurrentHi = new float[2];
            public float[] CoefsCurrentLo = new float[2];

            public string COM_CA6250;
            public string COM_Di30R;
            public string TCP_IP;
            public string Access_Path;
            public string Access_Table;
            public float MaxU;
            public float MaxI;
            public int StabilizationTime;
        }

        public event RoutedEventHandler ChangesMade;
        
        public float[] ControlVolts
        { get { return _control_volts; }}
        public float[] CoefsVoltage
        { get { return _coefs_voltage; } }
        public float[] CoefsCurrentHi
        { get { return _coefs_current_hi; } }
        public float[] CoefsCurrentLo
        { get { return _coefs_current_lo; } }
        public string Access_Path
        {
            get { return txtAccess_Path.Text; }
            set { txtAccess_Path.Text = value; }
        }
        public string Access_Table
        {
            get { return txtAccess_Table.Text; }
            set { txtAccess_Table.Text = value; }
        }
        public string AdamIP
        {
            get { return txtAdamIP.Text; }
            set { txtAdamIP.Text = value; }
        }
        public float MaxU
        { get { return _maxU; } }
        public float MaxI
        { get { return _maxI; } }
        public int StabilizationTime
        { get { return _stabilization_time; } }
        public string COM_CA6250
        {
            get { return cbCOM_CA6250.SelectedItem.ToString(); }
            set { cbCOM_CA6250.SelectedItem = value; }
        }
        public string COM_Di30R
        {
            get { return cbCOM_Di30r.SelectedItem != null ? cbCOM_Di30r.SelectedItem.ToString() : ""; }
            set { cbCOM_Di30r.SelectedItem = value; }
        }


        public SettingPage(string SettingsFile)
        {
            InitializeComponent();
            
            default_marginSave = bdrSave.Margin;
            pressed_marginSave.Right = default_marginSave.Right - 1;
            pressed_marginSave.Bottom = default_marginSave.Bottom - 1;
            
            default_marginClose = bdrClose.Margin;
            pressed_marginClose.Right = default_marginClose.Right - 1;
            pressed_marginClose.Top = default_marginClose.Top + 1;

            cbCOM_Di30r.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
            cbCOM_CA6250.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();

            _settingsFile = SettingsFile;
            
            if (!Deserialize(_settingsFile)) return;

            FillTables();
        }        

        private void FillTables()
        {
            FillTable_Coeffs();
            FillTable_AdamChannels();                
        }
        private void FillTable_Coeffs()
        {
            tableCoeffs.SetTableHeader("");
            tableCoeffs.SetRowHeader(0, "12 кВ.");
            tableCoeffs.SetRowHeader(1, "18 кВ.");

            tableCoeffs.SetColumnHeader(0, "Упр. напряжение");
            tableCoeffs.SetColumnHeader(1, "Коэф. напряжения");
            tableCoeffs.SetColumnHeader(2, "Коэф. ток(точный)");
            tableCoeffs.SetColumnHeader(3, "Коэф. ток(грубый)");

            for (int i = 0; i < 2; i++)
            {
                tableCoeffs.SetCellValue(i, 0, _control_volts[i].ToString());
                tableCoeffs.SetCellValue(i, 1, _coefs_voltage[i].ToString());
                tableCoeffs.SetCellValue(i, 2, _coefs_current_hi[i].ToString());
                tableCoeffs.SetCellValue(i, 3, _coefs_current_lo[i].ToString());
            }

            txtStabilizationTime.Text = _stabilization_time.ToString();
        }
        private void FillTable_AdamChannels()
        {
            using (xEquipment.xAdam5000TCP temp = new xEquipment.xAdam5000TCP())
            {
                if (!temp.LoadSettings(Vars.path_AdamCFG)) return;

                AdamIP = temp.ChannelInfos.IP;

                tableAdamInfo.SetTableHeader("Имя");
                tableAdamInfo.SetRowHeader(0, "Слот");
                tableAdamInfo.SetRowHeader(1, "Канал");
                tableAdamInfo.SetColumnHeader(0, "R AB");
                tableAdamInfo.SetColumnHeader(1, "R BC");
                tableAdamInfo.SetColumnHeader(2, "R CA");
                tableAdamInfo.SetColumnHeader(3, "START / STOP");
                tableAdamInfo.SetColumnHeader(4, "PRINT");
                tableAdamInfo.SetColumnHeader(5, "Маяк");

                for (int i = 0; i < Vars.AdamChannelNames.Length; i++)
                {
                    tableAdamInfo.SetCellValue(0, i, temp.ChannelInfos.GetChannel(Vars.AdamChannelNames[i]).Slot.ToString());
                    tableAdamInfo.SetCellValue(1, i, temp.ChannelInfos.GetChannel(Vars.AdamChannelNames[i]).Channel.ToString());
                }
            }
        }


        private void SaveTables()
        {
            SaveTable_Coeffs();
            SaveTable_AdamChannels();
        }
        private void SaveTable_Coeffs()
        {
            for (int i = 0; i < 2; i++)
            {
                _control_volts[i] = StringToFloat(tableCoeffs.GetCellValue(i, 0));
                _coefs_voltage[i] = StringToFloat(tableCoeffs.GetCellValue(i, 1));
                _coefs_current_hi[i] = StringToFloat(tableCoeffs.GetCellValue(i, 2));
                _coefs_current_lo[i] = StringToFloat(tableCoeffs.GetCellValue(i, 3));
            }

            if (!int.TryParse(txtStabilizationTime.Text, out _stabilization_time))
                _stabilization_time = 30;
        }
        private float StringToFloat(string text)
        {
            string s = xLibrary.xFunctions.String_FixDecimalSeparator(text);
            float result = 0;
            if (float.TryParse(s, out result)) return result;
            else return 0;
        }
        private void SaveTable_AdamChannels()
        {
            using (xEquipment.xAdam5000TCP temp = new xEquipment.xAdam5000TCP())
            {
                try
                {
                    temp.ChannelInfos = new xEquipment.xAdam5000TCP.Settings();
                    temp.ChannelInfos.IP = txtAdamIP.Text;
                    temp.ChannelInfos.Infos = new xEquipment.xAdam5000TCP.Settings.Info[Vars.AdamChannelNames.Length];
                    for(int i=0;i<temp.ChannelInfos.Infos.Length;i++)
                    {
                        temp.ChannelInfos.Infos[i] = new xEquipment.xAdam5000TCP.Settings.Info()
                        {
                            Name = Vars.AdamChannelNames[i],
                            Slot = int.Parse(tableAdamInfo.GetCellValue(0, i)),
                            Channel = int.Parse(tableAdamInfo.GetCellValue(1, i))
                        };
                    }

                    temp.SaveSettings(Vars.path_AdamCFG);
                }
                catch (Exception ex) { }
            }
        }


        private void bdrClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bdrClose.Margin = pressed_marginClose;
        }
        private void bdrClose_MouseUp(object sender, MouseButtonEventArgs e)
        {
            bdrClose.Margin = default_marginClose;        
            owner.Close();
        }
        private void bdrSave_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bdrSave.Margin = pressed_marginSave;
        }
        private void bdrSave_MouseUp(object sender, MouseButtonEventArgs e)
        {
            bdrSave.Margin = default_marginSave;
            if (Serialize(_settingsFile) && Deserialize(_settingsFile)) SettingsChanged = false;
            if (ChangesMade != null) ChangesMade(this, new RoutedEventArgs());
        }


        private void btnAccess_Path_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.InitialDirectory = Environment.CurrentDirectory;
            dialog.Filter = "Файл БД Access (*.mdb)|*.mdb|Все файлы (*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Access_Path = dialog.FileName.Replace(Environment.CurrentDirectory, @".");
            
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            SettingsChanged = true;
        }


        private bool Serialize(string fullPath)
        {
            try
            {
                SaveTables();

                _settings.TCP_IP = AdamIP;
                _settings.Access_Path = Access_Path;
                _settings.Access_Table = Access_Table;
                _settings.ControlVolts = _control_volts;
                _settings.CoefsVoltage = _coefs_voltage;
                _settings.CoefsCurrentHi = _coefs_current_hi;
                _settings.CoefsCurrentLo = _coefs_current_lo;
                _settings.MaxU = _maxU;
                _settings.MaxI = _maxI;
                _settings.StabilizationTime = _stabilization_time;
                _settings.COM_CA6250 = cbCOM_CA6250.SelectedItem.ToString();
                _settings.COM_Di30R = cbCOM_Di30r.SelectedItem.ToString();


                return xLibrary.xFunctions.Serialize<Settings>(_settings, fullPath);
            }
            catch (Exception ex) { return false; }
        }
        private bool Deserialize(string fullPath)
        {
            try
            {
                _settings = xLibrary.xFunctions.Deserialize<Settings>(fullPath);
                if (_settings == null) return false;

                cbCOM_CA6250.SelectedItem = _settings.COM_CA6250;
                cbCOM_Di30r.SelectedItem = _settings.COM_Di30R;
                AdamIP = _settings.TCP_IP;
                Access_Path = _settings.Access_Path;
                Access_Table = _settings.Access_Table;
                _control_volts = _settings.ControlVolts;
                _coefs_voltage = _settings.CoefsVoltage;
                _coefs_current_hi = _settings.CoefsCurrentHi;
                _coefs_current_lo = _settings.CoefsCurrentLo;
                _maxU = _settings.MaxU;
                _maxI = _settings.MaxI;
                _stabilization_time = _settings.StabilizationTime;
                
                return true;
            }
            catch (Exception ex) { return false; }
        }
        sealed class CustomizedBinder : System.Runtime.Serialization.SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type returntype = null;
                string sharedAssemblyName = "SharedAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                assemblyName = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                typeName = typeName.Replace(sharedAssemblyName, assemblyName);
                returntype =
                        Type.GetType(String.Format("{0}, {1}",
                        typeName, assemblyName));

                return returntype;
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
                assemblyName = "SharedAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            }
        }
        
    }
}
