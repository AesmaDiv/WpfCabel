using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPF_Try
{
    class xCOM
    {
        private SerialPort _port;
        private int _timeout = 250;
        private delegate byte[] PrepareMessage();
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

        /* ******************************************************************************************************* */
        public bool Connect([Optional] string port_name,
                            [Optional] int baudrate,
                            [Optional] Parity parity,
                            [Optional] int databits,
                            [Optional] StopBits stopbits)
        {
            if (IsConnected) return true;
            try
            {
                if (_port == null)
                {
                    if (port_name == null) return false;

                    _port = new SerialPort(port_name);
                    _port.BaudRate = baudrate == 0 ? 115200 : baudrate;
                    _port.Parity = parity == Parity.None ? Parity.None : parity;
                    _port.DataBits = databits == 0 ? 8 : databits;
                    _port.StopBits = stopbits == 0 ? StopBits.One : stopbits;
                }

                _port.Open();

                return _port.IsOpen;
            }
            catch (Exception ex) { _port = null; return false; }
        }
        public void Disconnect()
        {
            if (IsConnected) _port.Close();
        }
        /* ******************************************************************************************************* */
        public string Send(string message)
        {
            try
            {
                _input = Encoding.ASCII.GetBytes(message + '\r');
                Send(_input);

                return Encoding.ASCII.GetString(_output);
            }
            catch(Exception ex) { return null; }
        }
        public byte[] Send(byte[] input)
        {
            if (input == null) return null;
            if (input.Length == 0) return null;
            if (!IsConnected) return null;

            try
            {
                _input = input;
                Thread _thread = new Thread(new ThreadStart(Communicate));
                _thread.Start();
                _thread.Join();

                return _output;
            }
            catch(Exception ex) { return null; }
        }
        /* ******************************************************************************************************* */
        private async void Communicate()
        {
            _output = new byte[0];
            _port.Write(_input, 0, _input.Length);
            await GetAnswer();
            _input = new byte[0];
        }
        private async Task GetAnswer()
        {
            int wait = 0;
            do
            {
                if (_port.BytesToRead > 0)
                {
                    Array.Resize<byte>(ref _output, _output.Length + 1);
                    _output[_output.Length - 1] = (byte)_port.ReadByte();
                    if (_port.BytesToRead == 0) break;
                }
                else
                {
                    Thread.Sleep(1);
                    wait++;
                }
            }
            while (wait < _timeout);
        }
        /* ******************************************************************************************************* */
        
    }
}
