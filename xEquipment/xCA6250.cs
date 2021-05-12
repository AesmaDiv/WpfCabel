using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace xEquipment
{
    public class xCA6250 : xSerialBase
    {
        /*
        Список ошибок:
        1
        2
        3
        4
        5
        6
        7
        8
        9
        10
        11
        12
        13
        14
        15
        16
        17
        18
        19
        20

        */
        private CA6250_EventArgs _args = new CA6250_EventArgs();
        public event EventHandler<CA6250_EventArgs> OnEvent;
        public class CA6250_EventArgs : EventArgs
        {
            public string Message = "";
            public float Value = 0;
        }

        public xCA6250()
        {
            this.Mode = CommunicationMode.Classic ;
            this.BaudRate = 9600;
            this.Parity = Parity.None;
            this.DataBits = 8;
            this.StopBits = StopBits.One;
            this.PerByteSleep = 1;
            this.DataReceived += xCA6250_DataReceived;
        }
        
        private void ProcessRecievedData(byte[] bytes)
        {
            if(bytes.Length < 42) return;
            string message = Encoding.ASCII.GetString(bytes);
            if (!message.EndsWith(base.CaretReturn + base.NewLine)) return;
            if(!message.Contains("ERR"))
            {
                message = message.Replace(" ", "");//.Replace("\r\n", "");
                _args.Value = xLibrary.xFunctions.GetDecimalValue(message);
                if (message.Contains("mOhm")) _args.Value *= 0.001f;
                _args.Message = _args.Value == -1 ? "Wrong format" : "Success";
            }
            else
            {
                _args.Message = "Error " + message.Substring(3, 2);
            }
            BroadcastEvent();
        }
        private void BroadcastEvent()
        {
            if (OnEvent != null) OnEvent(this, _args);
        }
        private void xCA6250_DataReceived(object sender, xSerial_EventArgs e)
        {
            ProcessRecievedData(e.Bytes);
        }
    }
}
