using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace xLibrary
{
    public class xClient
    {
        private TcpClient _client = null;                               // TCP-клиент (собственно)
        private object _obj_lock = new object();
        private string _message = "";                                   // Сообщение для трансляции с событием
        private bool _connected = false;                                // флаг подлкючения
        private bool _listening = false;                                // флаг прослушивания
        private Communication _communication = Communication.Mobbus;    // Способ коммуникации Modbus или ACSII
        public int Port = 1509;                                         // Порт по умолчанию
        public bool last_data_still_processing = false;

        public enum Communication { Mobbus, ASCII };
        /// <summary>
        /// Событие
        /// </summary>
        public event EventHandler xEvent;
        /// <summary>
        /// Аргументы события
        /// </summary>
        public class ClientEventArgs : EventArgs
        {
            public byte[] Buffer;   // Массив байт значений
            public string Message;  // Сообщение
            public bool State;      // Флаг состояния (нафиг нужен??? мож удалить??)
        }
        /// <summary>
        /// Состояние подключения клиента
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_client == null) return false;
                if (_client.Client == null) return false;
                return _client.Connected;
            }
        }
        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="local_ip">локальный IP</param>
        /// <param name="communication">способ коммуникации</param>
        public xClient(IPAddress local_ip, Communication communication)
        {
            try
            {
                if (local_ip == null) return;
                if (_client != null) Disconnect();
                _communication = communication;
            }
            catch (ArgumentNullException e)
            { _message = "ArgumentNullException"; }
            catch (SocketException e)
            { _message = "SocketException"; }
            catch (System.IO.IOException e)
            { _message = "IOException"; }
        }
        /// <summary>
        /// Подлкючение
        /// </summary>
        /// <param name="ip">удаленный IP</param>
        /// <param name="port">удалённый порт</param>
        public void Connect(IPAddress ip, int port)
        {
            try
            {
                if (ip == null) return;
                // Создаю клиент и подлключаюсь
                _client = new TcpClient();
                var result = _client.BeginConnect(ip, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                if (!success) throw new SocketException();
                _client.EndConnect(result);


                //_client.Connect(ip, port);
                // Проверяю состояние подключения              
                _connected = _client.Connected;
                // Если подключиться не удалось..
                if (!_connected)
                {
                    // Транслирую событие и выхожу
                    BroadcastMessage("Unable to connect to Server", false);
                    return;
                }
                // Запускаю цикл прослушивания порта
                _listening = true;
                new Thread(Listen_Thread).Start();

                return;
            }
            catch (ArgumentNullException e)
            { _message = "ArgumentNullException"; }
            catch (SocketException e)
            { _message = "SocketException"; }
            catch (System.IO.IOException e)
            { _message = "IOException"; }

        }
        /// <summary>
        /// Отключение
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // Прекращаю прослушивание порта и жду
                if (_listening)
                {
                    _listening = false;
                    while (_connected) { }
                }
                if (_client == null) return;
                // Отключаюсь и освобождаю ресурсы
                //_client.GetStream().Close();
                //_client.Close();
                _client.Client.Disconnect(true);
                ((IDisposable)_client).Dispose();
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// Запись в порт 
        /// </summary>
        /// <param name="message">строка для записи</param>
        public void Send(string message)
        {
            try
            {
                // Получаю байты из строки
                byte[] data = new byte[256];
                data = Encoding.ASCII.GetBytes(message);
                // Записываю в порт
                Send(data);
            }
            catch (ArgumentNullException e)
            { message = "ArgumentNullException"; }
            catch (SocketException e)
            { message = "SocketException"; }
            catch (System.IO.IOException e)
            { message = "IOException"; }
        }
        /// <summary>
        /// Запись в порт
        /// </summary>
        /// <param name="buffer">байты для записи</param>
        public void Send(byte[] buffer)
        {
            try
            {
                _client.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (ArgumentNullException e)
            { /*Message = "ArgumentNullException";*/ }
            catch (SocketException e)
            { /*Message = "SocketException";*/ }
            catch (System.IO.IOException e)
            { /*Message = "IOException";*/ }
        }
        /// <summary>
        /// Получение списка локальных IP
        /// </summary>
        /// <returns>внезапно, список локальных IP</returns>
        public static IPAddress[] GetLocalIPs()
        {
            return Dns.GetHostAddresses(Dns.GetHostName());
        }
        /// <summary>
        /// Получение CRC-суммы
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="CRC16">ссылка на выходной массив для CRC-суммы</param>
        public static void GetCRC16(byte[] message, ref byte[] CRC16)
        {
            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF;
            byte CRCLow = 0xFF;
            char CRCLSB;
            for (int i = 0; i < (message.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);
                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);
                    if (CRCLSB == 1)
                    {
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                    }
                }
                CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
                CRCLow = (byte)(CRCFull & 0xFF);
            }
            CRC16[1] = CRCHigh;
            CRC16[0] = CRCLow;
        }
        public static ushort ToBigEndian(ushort value)
        {
            return (ushort)IPAddress.HostToNetworkOrder((short)value);
        }
        public static ushort[] ToBigEndian(ushort[] values)
        {
            ushort[] result = new ushort[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = ToBigEndian(values[i]);

            return result;
        }


        /// <summary>
        /// Поток прослушивания порта
        /// </summary>
        /// <param name="obj"></param>
        private void Listen_Thread(object obj)
        {
            
            // Пока флаг прослушивания активен
            while (_listening)
            {
                try
                {
                    // Если на входе нет байт - жду дальше
                    if (_client.Available == 0) continue;
                    if (last_data_still_processing) continue;
                    // Принимаю данные с порта
                    byte[] buffer = new byte[_client.Available];
                    _client.GetStream().Read(buffer, 0, buffer.Length);
                    // Транслирую пришедшие данные с событием
                    if (_communication == Communication.ASCII) BroadcastMessage(Encoding.ASCII.GetString(buffer, 0, buffer.Length), _connected);
                    else BroadcastMessage(buffer, "New data arrived", _connected);
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    _message = "Exception";
                    Disconnect();
                }
            }
            // Дизактивируем флаг подключения
            _connected = false;
        }
        /// <summary>
        /// Трансляция события
        /// </summary>
        /// <param name="State"></param>
        private void BroadcastMessage(bool State)
        {
            BroadcastMessage("", State);
        }
        private void BroadcastMessage(byte[] Buffer, bool State)
        {
            BroadcastMessage(Buffer, "", State);
        }
        private void BroadcastMessage(string Message, bool State)
        {
            BroadcastMessage(null, Message, State);
        }
        private void BroadcastMessage(byte[] Buffer, string Message, bool State)
        {
            if (xEvent != null)
            {
                ClientEventArgs args = new ClientEventArgs();
                args.Buffer = Buffer;
                args.Message = Message;
                args.State = State;
                xEvent(this, args);
            }
        }
    }
}
