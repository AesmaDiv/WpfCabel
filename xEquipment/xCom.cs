using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xEquipment
{
    public class xCom : IDisposable
    {
        public readonly string CaretReturn = new string((char)0x0D, 1);
        public readonly string NewLine = new string('\n', 1);
        public readonly string NullChar = new string('\0', 1);

        private SerialPort _port;
        private int _timeout = 250;
        private byte[] _input = new byte[0];
        private byte[] _output = new byte[0];

        public bool IsConnected
        {
            get
            {
                if (_port == null) return false;
                return _port.IsOpen;
            }
        }
        
        public bool Init(string port_name,
                         [Optional] int baudrate,
                         [Optional] Parity parity,
                         [Optional] int databits,
                         [Optional] StopBits stopbits)
        {
            try
            {
                if (port_name == null) return false;

                _port = new SerialPort(port_name);
                _port.BaudRate = baudrate == 0 ? 115200 : baudrate;
                _port.Parity = parity == Parity.None ? Parity.None : parity;
                _port.DataBits = databits == 0 ? 8 : databits;
                _port.StopBits = stopbits == 0 ? StopBits.One : stopbits;

                return true;
            }
            catch(Exception ex) { return false; }
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (IsConnected) Disconnect();
            _port.Dispose();
            _port = null;
        }


        public bool Connect()
        {
            if (IsConnected) return true;
            if (_port == null) return false;
            try { _port.Open(); return _port.IsOpen; }
            catch (Exception ex) { return false; }
        }
        public void Disconnect()
        {
            if (IsConnected) _port.Close();
        }
        private async Task<bool> Connect_Async()
        {
            return Connect();
        }
        private async Task Disconnect_Async()
        {
            Disconnect();
        }
        
        
        public string Send(string message, bool async)
        {
            byte[] input = Encoding.ASCII.GetBytes(message);
            Send(input, async);

            return Encoding.ASCII.GetString(_output);
        }
        public byte[] Send(byte[] input, bool async)
        {
            if (input == null) return null;
            if (input.Length == 0) return null;

            if ((!async)&&(!IsConnected)) return null;

            _input = input;

            Thread _thread = new Thread(async ? new ThreadStart(Send_Async_Thread) : new ThreadStart(Send_Thread));
            _thread.Start();
            _thread.Join();

            return _output;
        }

        private async void Send_Thread()
        {
            _output = new byte[0];
            try
            {
                if (_port.IsOpen)
                {
                
                    _port.Write(_input, 0, _input.Length);
                    await GetAnswer();
                } 
            }
            catch (Exception ex) { }
            _input = new byte[0];
        }
        private async void Send_Async_Thread()
        {
            bool result = await Connect_Async();
            if (!result) return;

            Send_Thread();
            try { await Disconnect_Async(); }
            catch(Exception ex) { }
        }

        private async Task GetAnswer()
        {
            int wait = 0;
            do
            {
                Thread.Sleep(1);
                if (_port.BytesToRead > 0)
                {
                    Array.Resize<byte>(ref _output, _output.Length + 1);
                    _output[_output.Length - 1] = (byte)_port.ReadByte();
                    if (_port.BytesToRead == 0) break;
                }
                else
                {
                    wait++;
                }
            }
            while (wait < _timeout);
        }        
    }
}
