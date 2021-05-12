using System;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xEquipment
{
    public class xSerialBase : IDisposable
    {
        protected SerialPort serial = new SerialPort();
        public enum CommunicationMode { Listener, QuestionAnswer, Classic }
        public readonly string CaretReturn = new string((char)0x0D, 1);
        public readonly string NewLine = new string('\n', 1);
        public readonly string NullChar = new string('\0', 1);

        private CommunicationMode _mode = CommunicationMode.QuestionAnswer;
        private int _recieve_timeout = 250;

        private byte[] _recieved_bytes = new byte[0];
        private int _reconnect_tries = 0;
        private int _per_byte_sleep_msec = 0;
        private Object _lock = new Object();
        private bool _is_active = false;
        private bool _is_stopped = false;

        private const int _MAX_RECONNECT_TRIES = 10;
        

        #region Параметры порта
        public string PortName
        {
            get { return serial.PortName; }
            set { if (!serial.IsOpen) serial.PortName = value; }
        }
        public int BaudRate
        {
            get { return serial.BaudRate; }
            set { serial.BaudRate = value; }
        }
        public Parity Parity
        {
            get { return serial.Parity; }
            set { serial.Parity = value; }
        }
        public int DataBits
        {
            get { return serial.DataBits; }
            set { serial.DataBits = value; }
        }
        public StopBits StopBits
        {
            get { return serial.StopBits; }
            set { serial.StopBits = value; }
        }
        public CommunicationMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }
        public int ReceiveTimeout
        {
            get { return _recieve_timeout; }
            set { _recieve_timeout = value; }
        }
        public int PerByteSleep
        {
            get { return _per_byte_sleep_msec; }
            set { _per_byte_sleep_msec = value; }
        }
        public bool IsConnected
        { get { return (serial != null) && serial.IsOpen; } }
        public event EventHandler<xSerial_EventArgs> DataReceived;
        public class xSerial_EventArgs : EventArgs
        {
            public byte[] Bytes;
        }
        #endregion

        public xSerialBase()
        {
            serial.BaudRate = 9600;
            serial.Parity = Parity.None;
            serial.DataBits = 8;
            serial.StopBits = StopBits.One;
        }
        public xSerialBase(CommunicationMode mode)
        {
            serial.BaudRate = 9600;
            serial.Parity = Parity.None;
            serial.DataBits = 8;
            serial.StopBits = StopBits.One;
            _mode = mode;
        }
        public void Connect()
        {
            if (serial == null) return;
            if (serial.IsOpen) return;

            serial.Open();
            if(serial.IsOpen)
                switch(_mode)
                {
                    case CommunicationMode.Listener:
                        _is_active = true;
                        _is_stopped = false;
                        new Thread(Listener_Thread).Start();
                        break;
                    case CommunicationMode.Classic:
                        serial.DataReceived += Serial_DataReceived;
                        break;
                }
        }

        
        public void Disconnect()
        {
            if (serial == null) return;
            if (!serial.IsOpen) return;
            _is_stopped = true;
            while (_is_active) { }
            serial.Close();
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual string Send(string command)
        {
            if (_mode == CommunicationMode.Listener) serial.Write(command);
            else
            {
                string result = "";

                serial.Write(command);
                result = Encoding.ASCII.GetString(GetAnswer());
                //result = result.Replace(_CR, "").Replace(_NullChar, "");

                return result;
            }
            return null;
        }
        protected virtual string Send_Async(string command)
        {
            Thread thread = new Thread(Asking_Thread);
            thread.Start(command);
            thread.Join();

            return async_result;
        }
        protected virtual byte[] Send(byte[] bytes, [Optional]int offset, [Optional]int count)
        {
            if(_mode == CommunicationMode.Listener) serial.Write(bytes, offset, count);
            else
            { 
                byte[] result = new byte[0];
                count = count == 0 ? count = bytes.Length : count;
                
                serial.Write(bytes, offset, count);
                result = GetAnswer();

                return result;
            }
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsConnected) Disconnect();
            serial.Dispose();
            serial = null;
        }
        protected virtual bool TryToReconnect()
        {
            _reconnect_tries++;
            if (_reconnect_tries < _MAX_RECONNECT_TRIES)
            {
                Connect();
                if (IsConnected) _reconnect_tries = 0;
                return IsConnected;
            }
            else return false;           
        }

        private byte[] GetAnswer()
        {
            byte[] result = new byte[0];
            int recieve_wait = 0;
            while (recieve_wait < _recieve_timeout)
            {
                try
                {
                    recieve_wait++;
                    if (serial.BytesToRead > 0)
                    {
                        result = ProcessIncomingData();
                        return result;
                    }
                    Thread.Sleep(1);
                }
                catch (Exception ex) { return result; }
            }
            return result;
        }
        private byte[] ProcessIncomingData()
        {
            int len = serial.BytesToRead;
            byte[] result = new byte[0];
            
            try
            {
                if(_per_byte_sleep_msec == 0)
                {
                    result = new byte[len];
                    serial.Read(result, 0, len);
                }
                else
                {
                    while (serial.BytesToRead > 0)
                    {
                        AddByteToArray(ref result, (byte)serial.ReadByte());
                        Thread.Sleep(_per_byte_sleep_msec);
                    }
                }
            }
            catch(Exception ex) { return new byte[0]; }

            return result;
        }
        

        private void Listener_Thread(object obj)
        {
            lock(_lock)
            {
                byte[] recieved_bytes = new byte[0];
                while (!_is_stopped)
                {
                    try {
                        Thread.Sleep(5);
                        if (serial.BytesToRead > 0)
                        {
                            recieved_bytes = ProcessIncomingData();
                            BroadcastEvent(recieved_bytes);
                        }
                    }
                    catch(Exception ex) { _is_stopped = true; }
                }
            }
            _is_active = false;
        }
        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            BroadcastEvent(ProcessIncomingData());
        }

        
        private void BroadcastEvent(byte[] bytes)
        {
            if (DataReceived != null) DataReceived(this, new xSerial_EventArgs() { Bytes = bytes });
        }
        private void AddByteToArray(ref byte[] array, byte value)
        {
            int length = array.Length;
            Array.Resize<byte>(ref array, ++length);
            array[--length] = value;
        }
        
        /* *************************************************************************************** */
        private string async_result = "";
        private async void Asking_Thread(object obj)
        {
            string command = obj as string;
            try
            {
                serial.Write(command);
                if (_mode == CommunicationMode.Listener) return;
                byte[] bytes = await GetAnswer_Async();
                async_result = Encoding.ASCII.GetString(bytes);
                //result = result.Replace(_CR, "").Replace(_NullChar, "");
            }
            catch (Exception ex) { async_result = "COM Error"; }
        }
        private async Task<byte[]> GetAnswer_Async()
        {
            byte[] result = new byte[0];
            int recieve_wait = 0;
            while (recieve_wait < _recieve_timeout)
            {
                try
                {
                    recieve_wait++;
                    if (serial.BytesToRead > 0)
                    {
                        result = await ProcessIncomingData_Async();
                        return result;
                    }
                    Thread.Sleep(1);
                }
                catch (Exception ex) { return result; }
            }
            return result;
        }
        private async Task<byte[]> ProcessIncomingData_Async()
        {
            int len = serial.BytesToRead;
            byte[] result = new byte[0];

            try
            {
                if (_per_byte_sleep_msec == 0)
                {
                    result = new byte[len];
                    serial.Read(result, 0, len);
                }
                else
                {
                    while (serial.BytesToRead > 0)
                    {
                        AddByteToArray(ref result, (byte)serial.ReadByte());
                        Thread.Sleep(_per_byte_sleep_msec);
                    }
                }
            }
            catch (Exception ex) { return new byte[0]; }

            return result;
        }
        /* *************************************************************************************** */
    }
}
