using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace xEquipment
{
    public class xDi30R : xCom
    {
        public const string READ_VALUES = "#01";
        public const string READ_VOLTS = "$028";
        public const string READ_FASES = "@03";
        public const string READ_FLAGS = "@04";

        public const string WRITE_VOLTS = "#02";
        public const string WRITE_FASES = "#03";
        public const string WRITE_FLAGS = "#04";

        public const float DISCHARGE_1_RESTRICTION = 12f; // по докам вообще нет
        public const float DISCHARGE_2_RESTRICTION = 6f; // по докам 6кВ
        public const float DISCHARGE_3_RESTRICTION = 2f; // по докам 2кВ   
        public const float HIGH_VOLTAGE_ALARM = 0.2f;
        public const float MAX_CONTROL_VOLTS = 1.80f;
        public const float MAX_VOLTAGE = 20.0f;

        private const int FASE_SWITCH_TIMEOUT = 250;

        public enum Fase { A, B, C, None };
        private enum CycleStage { ReadingValues, SetFases, SetFlags, SetControlVolts, GetFases, GetFlags, GetControlVolts };

        #region ПЕРЕМЕННЫЕ
        private bool _communication_as_text = true;

        private bool _is_reading = false;
        private bool _is_shutting_down = false;
        private Fase _active_fase = Fase.None;
        private CycleStage _cycle_stage = CycleStage.ReadingValues;

        private Thread _thread;
        private Object _lock = new Object();

        private float _coeff_control_volts = 0.062f;
        private float _coeff_voltage = 1;
        private float _coeff_current_hi = 1;
        private float _coeff_current_lo = 1;

        private string _CR = new string((char)0x0D, 1);
        private string _NullChar = new string('\0', 255);

        private Di30R_EventArgs _event_args = new Di30R_EventArgs();
        public event EventHandler<Di30R_EventArgs> OnEvent;
        public class Di30R_EventArgs : EventArgs
        {
            public float Voltage = 0;
            public float CurrentHi = 0;
            public float CurrentLo = 0;
            public float ControlVolts = 0;
            public BitArray Flags = new BitArray(8);
            public BitArray Fases = new BitArray(8);
            public string Message = "";
        }


        #endregion

        #region ССЫЛКИ НА ПЕРЕМЕННЫЕ
        public bool IsReading
        { get { return _is_reading; } }
        public bool IsShuttingDown
        { get { return _is_shutting_down; } }
        public float Voltage
        { get { return _event_args.Voltage; } }
        public float CurrentHi
        { get { return _event_args.CurrentHi; } }
        public float CurrentLo
        { get { return _event_args.CurrentLo; } }


        public float CoeffVoltage
        {
            get { return _coeff_voltage; }
            set { _coeff_voltage = value; }
        }
        public float CoeffCurrentHi
        {
            get { return _coeff_current_hi; }
            set { _coeff_current_hi = value; }
        }
        public float CoeffCurrentLo
        {
            get { return _coeff_current_lo; }
            set { _coeff_current_lo = value; }
        }
        public float CoeffControlVolts
        {
            get { return _coeff_control_volts; }
            set { _coeff_control_volts = value; }
        }

        public Fase ActiveFase
        {
            get
            {
                if (_event_args.Fases[0]) return Fase.A;
                if (_event_args.Fases[1]) return Fase.B;
                if (_event_args.Fases[2]) return Fase.C;
                return Fase.None;
            }
        }
        public float ControlVoltage
        {
            get { return _event_args.ControlVolts; }
            set
            {
                if (value >= MAX_CONTROL_VOLTS) return;
                if (value < 0) return;
                _event_args.ControlVolts = value;
                if (_is_reading) _cycle_stage = CycleStage.SetControlVolts;
                else Action_SetControlVolts();
                Console.WriteLine("    Di30R: установка контрольного напряжения " + value.ToString("0.###"));
            }
        }
        public bool DischargeStage1
        {
            get { return _event_args.Flags[0]; }
            set
            {
                _event_args.Flags[0] = value;
                if (_is_reading) _cycle_stage = CycleStage.SetFlags;
                else Action_SetFlag();
                Console.WriteLine("    Di30R: установка 1 ступени разрядки " + (value ? "Выкл":"Вкл"));
            }
        }
        public bool DischargeStage2
        {
            get { return _event_args.Flags[1]; }
            set
            {
                _event_args.Flags[1] = value;
                if (_is_reading) _cycle_stage = CycleStage.SetFlags;
                else Action_SetFlag();
                Console.WriteLine("    Di30R: установка 2 ступени разрядки " + (value ? "Выкл" : "Вкл"));
            }
        }
        public bool DischargeStage3
        {
            get { return _event_args.Flags[2]; }
            set
            {
                _event_args.Flags[2] = value;
                if (_is_reading) _cycle_stage = CycleStage.SetFlags;
                else Action_SetFlag();
                Console.WriteLine("    Di30R: установка 3 ступени разрядки " + (value ? "Выкл" : "Вкл"));
            }
        }
        public bool HighVoltage
        {
            get { return _event_args.Flags[3]; }
            set
            {
                _event_args.Flags[3] = value;
                if (_is_reading) _cycle_stage = CycleStage.SetFlags;
                else Action_SetFlag();
                Console.WriteLine("    Di30R: установка разрешения высокого напряжения " + (value ? "Вкл" : "Выкл"));
            }
        }
        #endregion

        public override void Dispose()
        {
            _event_args.Flags = new BitArray(0); _event_args.Flags = null;
            _event_args.Fases = new BitArray(0); _event_args.Fases = null;
            _event_args = null;

            base.Dispose();
        }

        /* **************************************************************************************************** */
        public string Send(string message, [Optional] bool is_crc_needed)
        {
            try
            {
                if (is_crc_needed) message += CalculateCRC(message);
                message += _CR;
                string result = base.Send(message, true);
                return result;
            }
            catch (Exception ex) { return "Exception: " + ex.Message; }
        }
        /* **************************************************************************************************** */
        public void SetDefaults()
        {
            // биты  8   7   6   5   4   3   2   1
            // упр. -/- -/- GrA GrB GrC  C   B   A
            // знач  0   0   0   0   0   0   0   0
            //if(IsConnected) Send("#030000");

        }
        public void SetFaseState(Fase fase, bool state)
        {
            if (fase == Fase.None) state = false;
            _active_fase = state ? fase : Fase.None;

            try
            {
                if (state) SetGround(_active_fase);
                else SetFase(_active_fase);
                Console.WriteLine("    Di30R: "  + (state ? "поднять с земли фазу " : "выключить из цепи фазу ") + fase.ToString());

                if (_is_reading) _cycle_stage = CycleStage.SetFases;
                else Action_SetFase();
                Thread.Sleep(FASE_SWITCH_TIMEOUT);

                if (state) SetFase(_active_fase);
                else SetGround(_active_fase);
                Console.WriteLine("    Di30R: " + (!state ? "заземлить фазу " : "включить в цепь фазу ") + fase.ToString());

                if (_is_reading) _cycle_stage = CycleStage.SetFases;
                else Action_SetFase();
                Thread.Sleep(FASE_SWITCH_TIMEOUT);
            }
            catch (Exception ex) { }
        }
        // биты  8   7   6   5   4   3   2   1
        // упр. -/- -/- GrA GrB GrC  C   B   A
        // знач  0   0   a   b   c   c   b   a 
        private void SetFase(Fase fase)
        {
            _event_args.Fases[0] = fase == Fase.A;
            _event_args.Fases[1] = fase == Fase.B;
            _event_args.Fases[2] = fase == Fase.C;
        }
        private void SetGround(Fase fase)
        {
            _event_args.Fases[3] = fase == Fase.C;
            _event_args.Fases[4] = fase == Fase.B;
            _event_args.Fases[5] = fase == Fase.A;
        }
        /* **************************************************************************************************** */
        public void StartReading()
        {
            if (_is_reading) return;
            if ((_thread != null) && (_thread.IsAlive)) return;

            Console.WriteLine("    Di30R: начало цикла чтения.");
            _is_reading = true;
            _thread = new Thread(Reading_Thread);
            _thread.Start();
        }
        public void StopReading()
        {
            _is_reading = false;
            if (_thread == null) return;
            while (_thread.IsAlive) { }
            Console.WriteLine("    Di30R: остановка цикла чтения.");
        }
        private void Reading_Thread(object obj)
        {
            try
            {
                while (_is_reading)
                {
                    Thread.Sleep(200);
                    switch (_cycle_stage)
                    {
                        //***************************************************//
                        #region УСТАНОВКА ЗНАЧЕНИЙ
                        case CycleStage.SetFases: Action_SetFase(); break;
                        case CycleStage.SetFlags: Action_SetFlag(); break;
                        case CycleStage.SetControlVolts: Action_SetControlVolts(); break;
                        #endregion
                        //***************************************************//
                        #region ПОЛУЧЕНИЕ ЗНАЧЕНИЙ
                        case CycleStage.GetFases: Action_GetFase(); break;
                        case CycleStage.GetFlags: Action_GetFlag(); break;
                        case CycleStage.GetControlVolts: Action_GetControlVolts(); break;
                        default: Update_Values(this.Send(READ_VALUES)); break;
                        #endregion
                        //***************************************************//
                    }
                    BroadcastEvent();
                }
            }
            catch (Exception ex) { }
        }
        /* **************************************************************************************************** */
        private void Action_SetFase()
        {
            Send(WRITE_FASES + BitsToString(_event_args.Fases));
            _cycle_stage = CycleStage.ReadingValues;
            Console.WriteLine("    Di30R: Отправка команды: переключение фазы");
        }
        private void Action_GetFase()
        {
            Update_BitArray(ref _event_args.Fases, this.Send(READ_FASES));
            _cycle_stage = CycleStage.ReadingValues;
        }
        private void Action_SetFlag()
        {
            Send(WRITE_FLAGS + BitsToString(_event_args.Flags));
            _cycle_stage = CycleStage.ReadingValues;
            Console.WriteLine("    Di30R: Отправка команды: установка флага");
        }
        private void Action_GetFlag()
        {
            Update_BitArray(ref _event_args.Flags, this.Send(READ_FLAGS));
            _cycle_stage = CycleStage.ReadingValues;
        }
        private void Action_SetControlVolts()
        {
            Send(WRITE_VOLTS + _event_args.ControlVolts.ToString("00.000"));            
           _cycle_stage = CycleStage.ReadingValues;
            Console.WriteLine("    Di30R: Отправка команды: установка управляющего напряжения");
        }
        private void Action_GetControlVolts()
        {
            Update_Volts(this.Send(READ_VOLTS));
            _cycle_stage = CycleStage.ReadingValues;
        }
        /* **************************************************************************************************** */
        public void SetDesireVoltage(float value)
        {
            if (!IsConnected) return;
            if (value > MAX_VOLTAGE) return;
            ControlVoltage = value * _coeff_control_volts;
        }
        public void UpdateStates()
        {
            Thread thread = new Thread(new ThreadStart(UpdateStates_Thread));
            thread.Start();
            thread.Join();
        }
        private void UpdateStates_Thread()
        {
            try
            {
                if (_is_reading)
                {
                    Thread.Sleep(1000);
                    _cycle_stage = CycleStage.GetFases;
                    Thread.Sleep(1000);
                    _cycle_stage = CycleStage.GetFlags;
                    Thread.Sleep(1000);
                    _cycle_stage = CycleStage.GetControlVolts;
                }
                else
                {
                    Update_BitArray(ref _event_args.Fases, this.Send(READ_FASES));
                    Update_BitArray(ref _event_args.Flags, this.Send(READ_FLAGS));
                    Update_Volts(this.Send(READ_VOLTS));

                    BroadcastEvent("UpdateStatesInfo");
                }
            }
            catch (Exception ex) { }
        }
        /* **************************************************************************************************** */
        public void Shutdown()
        {
            Console.WriteLine("    Di30R: Начало процесса разрядки");
            _is_shutting_down = true;
            new Thread(new ThreadStart(Shutdown_Reading_Thread)).Start();
        }
        private void Shutdown_Reading_Thread()
        {
            try
            {
                bool wait = true;
                bool send_zero = true;
                bool send_discharge1 = true;
                bool send_discharge2 = true;
                bool send_discharge3 = true;
                while (wait)
                {
                    if (send_zero) ControlVoltage = 0;
                    Thread.Sleep(1000);
                    if (Voltage < DISCHARGE_1_RESTRICTION)
                    {
                        if (send_discharge1) DischargeStage1 = false;
                        Thread.Sleep(1000);
                        if (Voltage < DISCHARGE_2_RESTRICTION)
                        {
                            if (send_discharge2)
                            {
                                send_zero = false;
                                send_discharge1 = false;
                                DischargeStage2 = false;
                            }
                            Thread.Sleep(1000);
                            if (Voltage < DISCHARGE_3_RESTRICTION)
                            {
                                if (send_discharge3)
                                {
                                    send_discharge2 = false;
                                    DischargeStage3 = false;
                                }
                                Thread.Sleep(1000);
                                if (Voltage < HIGH_VOLTAGE_ALARM)
                                {
                                    send_discharge3 = false;
                                    HighVoltage = false;
                                    wait = false;
                                }
                            }
                        }
                    }
                }
                
                _is_shutting_down = false;
                Console.WriteLine("    Di30R: завершение разрядки");
            }
            catch(Exception ex) { }
        }
        private void Shutdown_Reading_Thread_old()
        {
            try
            {
                bool wait = true;
                while (wait)
                {
                    ControlVoltage = 0;
                    Thread.Sleep(1000);
                    if (Voltage < DISCHARGE_1_RESTRICTION)
                    {
                        /*if (DischargeStage1*/
                        DischargeStage1 = false;
                        Thread.Sleep(1000);
                        if (Voltage < DISCHARGE_2_RESTRICTION)
                        {
                            /*if (DischargeStage2)*/
                            DischargeStage2 = false;
                            Thread.Sleep(1000);
                            if (Voltage < DISCHARGE_3_RESTRICTION)
                            {
                                /*if (DischargeStage3)*/
                                DischargeStage3 = false;
                                Thread.Sleep(1000);
                                if (Voltage < HIGH_VOLTAGE_ALARM)
                                {
                                    /*if (HighVoltage)*/
                                    HighVoltage = false;
                                    wait = false;
                                }
                            }
                        }
                    }
                }

                _is_shutting_down = false;
                Console.WriteLine("    Di30R: завершение разрядки");
            }
            catch (Exception ex) { }
        }
        /* **************************************************************************************************** */
        private void Update_BitArray(ref BitArray bit_array, string message)
        {
            try
            {
                if (!message.StartsWith(">")) return;
                if (!message.EndsWith(CaretReturn)) return;

                string temp = message.Replace(">", "");
                if (temp.Length < 2) return;
                byte b = Convert.ToByte(temp.Substring(2, 2));
                BitArray bits = new BitArray(new byte[] { b });

                if (bits.Length == 8) bit_array = bits;
            }
            catch (Exception ex) { }
        }
        private void Update_Values(string message)
        {
            try
            {
                _event_args.Message = message;
                if (!_event_args.Message.StartsWith(">")) return;
                if (!_event_args.Message.EndsWith(CaretReturn)) return;

                _event_args.Message = _event_args.Message.Replace("+", ";+").Replace("-", ";-").Replace(">;", "");

                string[] values = _event_args.Message.Split(';');

                float voltage = -1;
                float current_hi = -1;
                float current_lo = -1;
                
                _event_args.Voltage = float.TryParse(values[0], out voltage) ? voltage * _coeff_voltage : -1;
                _event_args.CurrentHi = float.TryParse(values[1], out current_hi) ? current_hi * _coeff_current_hi : -1;
                _event_args.CurrentLo = float.TryParse(values[2], out current_lo) ? current_lo * _coeff_current_lo : -1;

                if (current_hi > 40) _event_args.CurrentHi = _event_args.CurrentLo;
                //_event_args.Message = message;
            }
            catch (Exception ex) { }
        }
        private void Update_Volts(string message)
        {
            if (!message.StartsWith(">")) return;
            if (!message.EndsWith(CaretReturn)) return;

            try { _event_args.ControlVolts = float.Parse(message); }
            catch (Exception ex) { }
        }
        /* **************************************************************************************************** */
        private string BitsToString(BitArray bits)
        {
            byte[] result = new byte[1];

            try { bits.CopyTo(result, 0); }
            catch (Exception ex) { }
            string s = result[0].ToString("X04");
            return s;
        }
        private string BytesToString(byte[] bytes)
        {
            if (_communication_as_text) { return System.Text.Encoding.ASCII.GetString(bytes); }
            else
            {
                string result = "";
                foreach (byte b in bytes)
                    result += b.ToString("X02") + " ";

                return result;
            }
        }
        private string CalculateCRC(string message)
        {
            byte result = 0;
            try
            {
                foreach (char ch in message)
                    result += Convert.ToByte(ch);

                return result.ToString("X02");
            }
            catch (Exception ex) { return ""; }
        }
        /* **************************************************************************************************** */
        private void BroadcastEvent([Optional] string message)
        {
            if ((message != null)&&(message != "")) _event_args.Message = message;
            if (OnEvent != null) OnEvent(this, _event_args);
        }     
    }
}
