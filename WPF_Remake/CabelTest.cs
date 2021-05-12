using System;
using System.Timers;
using System.Threading;
using xEquipment;
using xLibrary;
using System.IO.Ports;
using System.Collections;

namespace WPF_Try
{
    public class CabelTest
    {
        public enum TestMode { Ohm, Hipot };
        public int VoltageSelection = 0;
        public TestMode CurrentTestMode = CabelTest.TestMode.Hipot;
        public xHipot Hipot = new xHipot();
        public xROhm ROhm = new xROhm();

        public bool IsROhmInterrupted = false;
        public bool IsHipotInterrupted = false;

        public bool AdamTCP_Connect()
        {
            try
            {
                if (!Vars.AdamTCP.LoadSettings(Vars.path_AdamCFG)) return false;
                Vars.AdamTCP.Connect();
                Vars.AdamTCP.SetReadingState(Vars.AdamTCP.IsConnected);
                return Vars.AdamTCP.IsConnected;
            }
            catch(Exception ex) { return false; }
        }
        public void AdamTCP_Disconnect()
        {
            try
            {
                Vars.AdamTCP.SetReadingState(false);
                Vars.AdamTCP.Disconnect();
                //Vars.AdamTCP.Dispose();
            }
            catch(Exception ex) { }
        }

        public void SwitchMayak(bool is_on)
        {
            xAdam5000TCP.Settings.Info info = Vars.AdamTCP.ChannelInfos.GetChannel("Mayak"); //Vars.AdamSettings.GetChannel("Mayak");
            Vars.AdamTCP.SetChannelValue_Digital(info.Slot, info.Channel, is_on);
        }
        

        public void Hipot_Start()
        {
            CurrentTestMode = CabelTest.TestMode.Hipot;
            Hipot.Start();
        }
        public void Hipot_Rescue()
        {
            //System.Windows.Controls.CheckBox ch = Vars.mainWindow.FindName("checkHipot_Reading") as System.Windows.Controls.CheckBox;
            //if (ch != null)
            //{
            //    if (ch.IsChecked == true) ch.IsChecked = false;
            //}
            //else if (Hipot.IsReading) Hipot.StopReading();
            
            //if (Hipot.IsConnected) Hipot.Disconnect();

            //if (!Hipot.IsConnected) Hipot.Connect(Vars.settings.COM_Di30R);

            //if (ch != null)
            //{
            //    if (ch.IsChecked == false) ch.IsChecked = true;
            //}
            //else if (!Hipot.IsReading) Vars.Test.Hipot.StartReading();

            //IsHipotInterrupted = true;
            //Hipot.Shutdown();
            //Hipot.ActiveFase = CabelData.HipotFase.None;
        }
        public void Hipot_MoveMarker(float seconds, float value_i)
        {
            try
            {
                //System.Drawing.PointF p_avr = Hipot.GetAverageUI();
                //p_avr.X = seconds / 60f;
                //p_avr.Y *= Vars.CurrentCabel.LengthCoeff;
                //if (p_avr.Y > Vars.settings.MaxI) p_avr.Y = Vars.settings.MaxI;
                //Vars.chart.MarkPosition = p_avr;
                int test_time = 60 * (Hipot.IsWithPolar ? Vars.TestTime_Long : Vars.TestTime_Short);
                if (seconds > test_time) seconds = test_time;
                if (value_i > Vars.settings.MaxI) value_i = Vars.settings.MaxI;
                System.Drawing.PointF point = new System.Drawing.PointF(seconds / 60f, value_i);
                Vars.chart.MarkPosition = point;
            }
            catch (Exception ex) { }
        }

        public class xHipot
        {
            private xDi30R _di30r = new xDi30R();
            private int _tmr_count = 0;

            private bool _with_polar = false;
            private bool _is_active = false;
            private bool _is_started = false;
            private bool _is_stopped = false;
            private bool _is_ended = true;
            private bool _can_continue = true;
            private bool _fix_point = false;

            private int _u_index = 0;
            private int _i_index = 0;
            private float[] _u = new float[0];
            private float[] _i = new float[0];

            private int _output_voltage_index = -1;
            private int _testing_time_seconds = 2 * 60;

            public CabelData.HipotFase ActiveFase
            {
                get { return (CabelData.HipotFase)_di30r.ActiveFase; }
                set { SwitchFase(value); }
            }
            public event EventHandler<xDi30R.Di30R_EventArgs> OnEvent;

            public bool IsConnected
            { get { return _di30r.IsConnected; } }
            public bool IsReading
            {
                get { return _di30r.IsReading; }
                set
                {
                    if (value) _di30r.StartReading();
                    else _di30r.StopReading();
                }
            }
            public bool IsActive
            { get { return _is_active; } }
            public bool IsEnded
            { get { return _is_ended; } }
            public bool IsWithPolar
            {
                get { return _with_polar; }
                set
                {
                    _with_polar = value;
                    _testing_time_seconds = 60 * (value ? Vars.TestTime_Long : Vars.TestTime_Short);
                }
            }

            public int TestingTime
            { get { return _testing_time_seconds; } }
            public int OutputVoltage
            {
                get { return _output_voltage_index; }
                set
                {
                    if ((value < 0) || (value > 1)) return;

                    _output_voltage_index = value;
                    _di30r.CoeffVoltage =   Vars.settings.CoefsVoltage[_output_voltage_index];
                    _di30r.CoeffCurrentHi = Vars.settings.CoefsCurrentHi[_output_voltage_index];
                    _di30r.CoeffCurrentLo = Vars.settings.CoefsCurrentLo[_output_voltage_index];
                    
                }
            }

            public bool Init(string port_name)
            { 
                _di30r.OnEvent += _di30r_OnEvent;
                try
                {
                    
                    if ((port_name == null) || (port_name == "")) return false;
                    _di30r.Init(port_name, 115200, Parity.None, 8, StopBits.One);

                    if (!_di30r.Connect()) return false;
                    _di30r.Disconnect();

                    SwitchFase(CabelData.HipotFase.None);
                    _di30r.UpdateStates();

                    return true;
                }
                catch (Exception ex) { return false; }
            }
            public void Disconnect()
            {
                if (!_di30r.IsConnected) return;
                if (_is_active) _di30r.Shutdown();
                _di30r.Disconnect();
            }
            public void UpdateStatesInfo()
            {
                _di30r.UpdateStates();
            }
            public void Shutdown()
            {
                if (!IsConnected) return;
                _di30r.Shutdown();
            }
            public void StartReading()
            {
                IsReading = true;
            }
            public void StopReading()
            {
                IsReading = false;
            }
            public bool Start()
            {
                //if (!_di30r.IsConnected) return false;
                if (!_di30r.IsReading) return false;

                if (Vars.settings.ControlVolts == null) return false;
                if (Vars.settings.ControlVolts.Length != 2) return false;

                if (_output_voltage_index < 0) return false;


                _tmr_count = 0;
                _is_active = true;
                _is_stopped = false;
                _is_ended = false;

                new Thread(Testing_18kV_Thread).Start();

                return true;
            }
            public void Stop()
            {
                _is_active = false;
            }

            public System.Drawing.PointF GetAverageUI()
            {
                return new System.Drawing.PointF(xFunctions.GetAverage(_u), xFunctions.GetAverage(_i));
            }

            private void Testing_18kV_Thread(object obj)
            {
                try
                {
                    Console.WriteLine("Начало испытания..");
                    do
                    {
                        // Задержка 1сек
                        Thread.Sleep(1000);
                        // Смотрю счётчик
                        switch (_tmr_count)
                        {
                            case 0:
                                break;
                            case 1:
                                // Выключаю реле разрядки 3 ступени
                                _di30r.DischargeStage3 = true;
                                Console.WriteLine("Выключаю реле разрядки 3 ступени");
                                break;
                            case 2:
                                // Выключаю реле разрядки 2 ступени
                                _di30r.DischargeStage2 = true;
                                Console.WriteLine("Выключаю реле разрядки 2 ступени");
                                break;
                            case 3:
                                // Выключаю реле разрядки 1 ступени
                                _di30r.DischargeStage1 = true;
                                Console.WriteLine("Выключаю реле разрядки 1 ступени");
                                break;
                            case 4:
                                // Выключаю реле защиты от высокого напряжения
                                _di30r.HighVoltage = true;
                                Console.WriteLine("Выключаю реле защиты от высокого напряжения");
                                // Жду пока напряжение не упадёт до безопасного (100В)
                                _can_continue = _di30r.Voltage < 0.1f;
                                Console.WriteLine("Жду пока напряжение не упадёт до безопасного (100В)");
                                break;
                            case 5:
                                _is_started = true;
                                break;
                            case 10:
                                // Устанавливаю напряжение 3 (4.5) кВ
                                _di30r.ControlVoltage = Vars.settings.ControlVolts[_output_voltage_index] * 0.25f;
                                Console.WriteLine("Устанавливаю напряжение 3 (4.5) кВ");
                                //_di30r.CoeffVoltage = Vars.settings.CoefsVoltage[_output_voltage_index];
                                //_di30r.CoeffCurrentHi = Vars.settings.CoefsCurrentHi[_output_voltage_index];
                                //_di30r.CoeffCurrentLo = Vars.settings.CoefsCurrentLo[_output_voltage_index];
                                break;
                            case 20:
                                // Устанавливаю напряжение 6 (9) кВ
                                _di30r.ControlVoltage = Vars.settings.ControlVolts[_output_voltage_index] * 0.5f;
                                Console.WriteLine("Устанавливаю напряжение 6 (9) кВ");
                                break;
                            case 30:
                                // Устанавливаю напряжение 9 (13.5) кВ
                                _di30r.ControlVoltage = Vars.settings.ControlVolts[_output_voltage_index] * 0.75f;
                                Console.WriteLine("Устанавливаю напряжение 9 (13.5) кВ");
                                break;
                            case 40:
                                // Устанавливаю напряжение 12 (18) кВ
                                _di30r.ControlVoltage = Vars.settings.ControlVolts[_output_voltage_index];
                                Console.WriteLine("Устанавливаю напряжение 12 (18) кВ");
                                break;
                        }
                        if (_can_continue) _tmr_count++;
                        if (_tmr_count > _testing_time_seconds + 70) _is_active = false;
                        else if (!_fix_point) _fix_point = (_tmr_count > 50) /*&& (_tmr_count <= _testing_time_seconds)*/;
                    } while (_is_active);
                    Console.WriteLine("Окончание испытания...");
                    _is_stopped = true;
                    _di30r.Shutdown();
                    while (_di30r.IsShuttingDown) { }
                }
                catch (Exception ex) { }
            }
            private void SwitchFase(CabelData.HipotFase fase)
            {
                _di30r.SetFaseState(xDi30R.Fase.None, true);

                if (fase != CabelData.HipotFase.None)
                    _di30r.SetFaseState((xDi30R.Fase)fase, true);
            }
            private void AddToAverage(float u, float i)
            {
                xFunctions.UpdateArray<float>(ref _u, u, ref _u_index, 10);
                xFunctions.UpdateArray<float>(ref _i, i, ref _i_index, 10);
            }
            private void ProcessReceivedData(xDi30R.Di30R_EventArgs args)
            {
                AddToAverage(args.Voltage, args.CurrentHi);
                if (_is_started)
                {
                    args.Message = "ResetTime";
                    Console.WriteLine("Сброс времени");
                    _is_started = false;
                }
                else if (_fix_point)
                {
                    args.Message = "FixPoint";
                    _fix_point = false;
                }
                else if (_is_stopped)
                {
                    args.Message = "TestStopped";
                    Console.WriteLine("Тест остановлен");
                    _is_stopped = false;
                }
                else if ((!_is_ended)&&(!_is_active)&&(args.Voltage < 0.050f))
                {
                    args.Message = "SafeVoltage";
                    _is_active = false;
                    _is_ended = true;
                }
                BroadcastEvent(args);
            }
            private void BroadcastEvent(xDi30R.Di30R_EventArgs args)
            {
                if (OnEvent != null) OnEvent(this, args);
            }
            private void _di30r_OnEvent(object sender, xDi30R.Di30R_EventArgs e)
            {
                ProcessReceivedData(e);
            }
        }

        public class xROhm
        {
            private xCA6250 _ca6250 = new xCA6250();
            private CabelData.ROhmFase _active_fase = CabelData.ROhmFase.AB;
            private bool _is_active = false;
            private Thread _thread_test;
            private object _thread_lock = new object();

            public bool IsActive
            { get { return _is_active; } }
            public CabelData.ROhmFase ActiveFase
            {
                get { return _active_fase; }
                set { SwitchActiveFase(value); }
            }
            public event EventHandler<xCA6250.CA6250_EventArgs> OnEvent;
            
            public bool Connect()
            {
                if (_ca6250.IsConnected) return true;

                _ca6250.OnEvent += _ca6250_OnEvent;
                try
                {
                    _ca6250.PortName = Vars.settings.cbCOM_CA6250.SelectedItem.ToString();
                    _ca6250.Connect();

                    Vars.mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                    { MainFunctions.Log("    ROhm - Подключение к микрометру"); }));

                    return _ca6250.IsConnected;
                }
                catch(Exception ex) { return false; }
            }
            public void Disconnect()
            {
                if (_ca6250.IsConnected) _ca6250.Disconnect();

                Vars.mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                { MainFunctions.Log("    ROhm - Отключение от микрометра"); }));

            }

            public void TestFase()
            {
                _is_active = true;
                _thread_test = new Thread(Testing_Thread);
                _thread_test.Start();
            }
            public void Abort()
            {
                if (_thread_test == null) return;
                if (_thread_test.IsAlive)
                {
                    _thread_test.Abort();
                    _is_active = false;
                }
            }

            public void Push_Start_Stop()
            {
                Start_Stop();
            }
            public void Push_Print()
            {
                Print();
            }

            private void Start_Stop()
            {

                new Thread(CA6250_Command_Thread).Start("CA6250_Start");
            }
            private void Print()
            {
                new Thread(CA6250_Command_Thread).Start("CA6250_Print");
            }

            private void SwitchActiveFase(CabelData.ROhmFase fase)
            {
                _active_fase = fase;
                SwitchDigitalChannel("ROhm_FaseA", fase == CabelData.ROhmFase.AB);
                SwitchDigitalChannel("ROhm_FaseB", fase == CabelData.ROhmFase.BC);
                SwitchDigitalChannel("ROhm_FaseC", fase == CabelData.ROhmFase.CA);
            }
            private void SwitchDigitalChannel(string info_name, bool state)
            {
                xAdam5000TCP.Settings settings = Vars.AdamTCP.ChannelInfos;
                if (settings == null) return;
                xAdam5000TCP.Settings.Info info = settings.GetChannel(info_name);//Vars.AdamSettings.GetChannel(info_name);
                Vars.AdamTCP.SetChannelValue_Digital(info.Slot, info.Channel, state);
            }

            private void Testing_Thread(object obj)
            {
                //lock (_thread_lock)
                //{
                    Start_Stop();
                    Thread.Sleep(Vars.ROhm_stabilization_time);
                    Start_Stop();
                    Thread.Sleep(Vars.ROhm_before_print_time);
                    Print();
                    Thread.Sleep(Vars.ROhm_after_print_time);
                    _is_active = false;
                //}
            }
            private void CA6250_Command_Thread(object obj)
            {
                //lock(_thread_lock)
                //{
                Vars.mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                { MainFunctions.Log("    ROhm - комманда " + obj.ToString()); }));

                SwitchDigitalChannel(obj.ToString(), true);
                Thread.Sleep(Vars.ROhm_contact_short_time);
                SwitchDigitalChannel(obj.ToString(), false);
                //}
            }
            private void ProcessReceivedData(xCA6250.CA6250_EventArgs args)
            {
                BroadcastEvent(args);
            }
            private void BroadcastEvent(xCA6250.CA6250_EventArgs args)
            {
                if (OnEvent != null) OnEvent(this, args);
            }
            private void _ca6250_OnEvent(object sender, xCA6250.CA6250_EventArgs e)
            {
                ProcessReceivedData(e);
            }
        }
    }
}
