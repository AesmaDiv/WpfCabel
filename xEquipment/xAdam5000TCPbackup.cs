using System;
using System.Collections;
using System.Net;
using System.Threading;
using System.IO;

namespace xEquipment
{
    public class xAdam5000TCP_Backup : IDisposable
    {
        private xClient _client;                  // TCP клиент для общения с адамом
        private Settings _settings;
        private ChannelsData _stream_data;        // структура-класс для хранения значений каналов
        private bool _reading = false;            // флаг - выполняется ли цикл чтения значений из адама
        private int _read_delay = 1000;           // пауза между запросами дискретных и аналоговых значений (мсек)
        private Timer _tmr_read = null;

        private byte[] _cmd_read_AIO = new byte[] // Команда запроса значений всех аналоговых каналов
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x06,
            0x01, 0x04, 0x00, 0x00, 0x00, 0x40
        };
        private byte[] _cmd_read_DIO = new byte[] // Команда запроса состояний всех дискретных каналов
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x06,
            0x01, 0x01, 0x00, 0x00, 0x00, 0x80
        };

        /// <summary>
        /// Событие
        /// </summary>
        public event EventHandler<AdamEventArgs> OnEvent;
        /// <summary>
        /// Аргументы события
        /// </summary>
        public class AdamEventArgs : EventArgs
        {
            public byte[] Buffer = new byte[0]; // Массив байт значений
            public string Message = "";         // Сообщение
            public bool State = false;          // Флаг состояния (нафиг нужен??? мож удалить??)
            public bool NewDataArrived = false; // Флаг получения новых данных
        }
        /// <summary>
        /// Состояние подключения клиента
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_client == null) return false;
                return _client.IsConnected;
            }
        }
        public int ReadDelay
        {
            get { return _read_delay * 2; }
            set { _read_delay = value / 2; }
        }
        public bool LastDataStillProcessing
        { set { _client.last_data_still_processing = value; } }
        public Settings ChannelInfos
        {
            get { return _settings; }
            set { _settings = value; }
        }
        public ChannelsData StreamData
        {  get { return _stream_data; } }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="local_ip">локальный IP</param>
        public xAdam5000TCP_Backup()
        {
            try
            {
                // Пробую получить локальный IP
                IPAddress[] ips = xClient.GetLocalIPs();
                IPAddress local_ip = ips[1];// Array.Find<IPAddress>(ips, ip => ip.GetAddressBytes()[1] != 0);
                if (local_ip == null) return;
                // Инициализирую TCP-клиент и привязываю функцию к событию TCP-клиента
                _client = new xClient(local_ip, xClient.Communication.Mobbus);
                _client.OnEvent += _client_xEvent;
                // Инициализирую структуру для хранения значений каналов
                _stream_data = new ChannelsData();
            }
            catch(Exception ex) { }
        }

        /// <summary>
        /// Уничтожение и очистка
        /// </summary>
        public void Dispose()
        {
            // Жду отключения
            while ((_client != null) && (_client.IsConnected))
            {
                Disconnect();
            }
            // Уничтожаю
            _client = null;
            _stream_data = null;
        }

        public bool LoadSettings(string filename)
        {
            if (!File.Exists(filename)) return false;
            try
            {
                _settings = xLibrary.xFunctions.Deserialize<Settings>(filename);
                return _settings != null;
            }
            catch (Exception ex) { return false; }
        }
        public bool SaveSettings(string filename)
        {
            try { return xLibrary.xFunctions.Serialize<Settings>(_settings, filename); }
            catch (Exception ex) { return false; }
        }

        #region ПОДКЛЮЧЕНИЕ-ОТКЛЮЧЕНИЕ
        // *******************************************************************************************************************************
        /// <summary>
        /// Подключение к адаму
        /// </summary>
        /// <param name="distant_ip">удалённый IP</param>
        public void Connect([System.Runtime.InteropServices.Optional]string distant_ip)
        {
            if ((distant_ip == null)||(distant_ip == "")) distant_ip = _settings.IP;
            Connect(IPAddress.Parse(distant_ip));
        }
        public void Connect(IPAddress distant_ip)
        {
            try
            {
                // Собственно..
                _client.Connect(distant_ip, 502);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// Отключение от адама
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // Прекращаю чтение
                _reading = false;
                // Собственно..
                _client.Disconnect();
            }
            catch (Exception ex) { }
        }
        // *******************************************************************************************************************************
        #endregion

        #region ЧТЕНИЕ
        // *******************************************************************************************************************************
        /// <summary>
        /// Установка состояния чтения
        /// </summary>
        /// <param name="state">ВКЛ / ВЫКЛ</param>
        public void SetReadingState(bool state)
        {
            // Устанавливаю флаг прослушивания
            _reading = state;
            // Запускаю поток запроса значений каналов
            //if (_reading) new Thread(ReadValues_Thread).Start();
            //_tmr_read = new Timer(ReadValues_Thread);
            //_tmr_read.Change(_reading ? 0 : Timeout.Infinite, _reading ? _read_delay : Timeout.Infinite);
        }
        // *******************************************************************************************************************************
        #endregion

        #region ИЗМЕНЕНИЕ ЗНАЧЕНИЙ КАНАЛОВ
        // *******************************************************************************************************************************
        /// <summary>
        /// Установка состояния дискретного
        /// </summary>
        /// <param name="slot">слот</param>
        /// <param name="channel">канал</param>
        public void SetChannelValue_Digital(int slot, int channel, bool value)
        {
            // Генерирую команду
            byte[] cmd_write_DIO = BuildCommand(ChannelsData.SlotType.DIO, slot, channel, value);
            // Запускаю поток записи (отправки команды в адам)
            new Thread(WriteValue_Thread).Start(cmd_write_DIO);
        }
        public void SetChannelValue_Digital(string name, bool value)
        {
            if (_settings == null) return;
            if (_settings.GetChannel(name).Equals(default(Settings.Info))) return;

            SetChannelValue_Digital(_settings.GetChannel(name).Slot, _settings.GetChannel(name).Channel, value);
        }
        /// <summary>
        /// Установка значения аналогового
        /// </summary>
        /// <param name="slot">слот</param>
        /// <param name="channel">канал</param>
        public void SetChannelValue_Analog(int slot, int channel, ushort value)
        {
            // Генерирую команду
            byte[] cmd_write_AIO = BuildCommand(ChannelsData.SlotType.AIO, slot, channel, value);
            // Запускаю поток записи (отправки команды в адам)
            new Thread(WriteValue_Thread).Start(cmd_write_AIO);
        }
        public void SetChannelValue_Analog(string name, ushort value)
        {
            if (_settings == null) return;
            if (_settings.GetChannel(name).Equals(default(Settings.Info))) return;

            SetChannelValue_Analog(_settings.GetChannel(name).Slot, _settings.GetChannel(name).Channel, value);
        }
        // *******************************************************************************************************************************
        #endregion

        #region ПОТОКИ
        // *******************************************************************************************************************************
        /// <summary>
        /// Запись команды в адам
        /// </summary>
        /// <param name="obj">объект для отправки</param>
        private void WriteValue_Thread(object obj)
        {
            try
            {
                // Преобразую аргумент функции в массив байт (ибо это таки массив байт и в кач-ве агрумента я таки передавал массив байт.. таки)
                byte[] cmd_write = (byte[])obj;
                // Отправляю массив в адам
                _client.Send(cmd_write);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// Чтение значений из адама
        /// </summary>
        /// <param name="obj"></param>
        private void ReadValues_Thread(object obj)
        {
            try
            {
                // Пока флаг чтения активен
                //while (_reading)
                //{
                    // Через паузы отправляю в адам комманды запроса..
                    //Thread.Sleep(_read_delay);
                    // ..значений аналоговых каналов
                    _client.Send(_cmd_read_AIO);
                    //Thread.Sleep(_read_delay);
                    // ..состояний дискретных каналов
                    _client.Send(_cmd_read_DIO);
                //}
            }
            catch (Exception ex) { }
        }
        // *******************************************************************************************************************************
        #endregion

        #region ВТОРОСТЕПЕННЫЕ ФУНКЦИИ
        // *******************************************************************************************************************************
        /// <summary>
        /// Парс байтов (ээээ.. ну или как правильно?)
        /// </summary>
        /// <param name="buffer">массив байт</param>
        private void ParseBytes(byte[] buffer)
        {
            try
            {
                // Если массив меньше 9 - выхожу.. ибо тут нет того что надо
                if (buffer.Length < 9) return;
                // Определяю границы
                int bytes_count = buffer[8];                    // по этому адресу лежит кол-во пришедших байт значений
                int values_count = bytes_count / sizeof(short); // это кол-во значений из расчёта (значение = 2 байта)
                // Если массив меньше нужной длины - выхожу.. неча тут ловить
                if (buffer.Length < 9 + bytes_count) return;
                // Создаю массив для значений
                ushort[] values = new ushort[values_count];
                // Беру из массива байт, только те что представляют собственно значения (начиная с 9-го)
                byte[] temp = xLibrary.xFunctions.TakeFromArray<byte>(buffer, 9, bytes_count);
                // Перевожу байты в значения, методом копирования памяти (крутяк))
                Buffer.BlockCopy(temp, 0, values, 0, temp.Length);
                // Обновляю значения каналов в структуре (если 7-ой байт(код команды) = 4, то это аналоговые, если = 1, то это дискретные)
                if (buffer[7] == 4) _stream_data.RefreshValues_Analog(values, true);
                if (buffer[7] == 1) _stream_data.RefreshValues_Digital(values, false);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// Генерирование команды для адама
        /// </summary>
        /// <param name="slot_type">тип слота (Аналоговый-Дискретный)</param>
        /// <param name="slot">слот</param>
        /// <param name="channel">канал</param>
        /// <param name="value">значение</param>
        /// <returns>команда</returns>
        private byte[] BuildCommand(ChannelsData.SlotType slot_type, int slot, int channel, object value)
        {
            // Получаю адресс нужного регистра в байтовом виде
            byte[] address = GetRegisterAddress(slot_type, slot, channel);
            // Получаю устанавливоемое значение в байтовом виде
            byte[] bytes = slot_type == ChannelsData.SlotType.AIO ?                       // Если канал аналоговый..
                           BitConverter.GetBytes((ushort)value) :                           // ..просто преобразую ushort в байты     
                           new byte[] { 0x00, (byte)((bool)value == true ? 0xFF : 0x00) };  // ..в.п.с. и так понятно
            // Формирую команду
            byte[] result = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x06,                                         // Заголовок (так надо)
                0x01,                                                                       // Modbus-адрес адама
                slot_type == ChannelsData.SlotType.DIO ? (byte)5 : (byte)6,               // Код команды
                address[1],                                                                 // Старший байт адреса
                address[0],                                                                 // Младший байт адреса
                bytes[1],                                                                   // Старший байт значения
                bytes[0]                                                                    // Младший байт значения
            };
            // Возвращаю результат
            return result;
        }
        /// <summary>
        /// Получение адреса канала
        /// </summary>
        /// <param name="slot_type">тип слота (Аналоговый-Дискретный)</param>
        /// <param name="slot">слот</param>
        /// <param name="channel">канал</param>
        /// <returns>адрес</returns>
        private byte[] GetRegisterAddress(ChannelsData.SlotType slot_type, int slot, int channel)
        {
            // Адреса каналов:
            // аналоговых 0x0001-0x0080 (00001-00128) смещение на слот 16
            // дискретных 0x0001-0x0040 (40001-40064) смещение на слот 8

            // Устанавливаю смещение
            int offset_coef = slot_type == ChannelsData.SlotType.AIO ? 8 : 16;
            // Получаю адрес
            ushort address = (ushort)(slot * offset_coef + channel);
            // Возвращаю в виде байтов
            return BitConverter.GetBytes(address);
        }
        // *******************************************************************************************************************************
        /// <summary>
        /// Cобытие TCP-клиента
        /// </summary>
        private void _client_xEvent(object sender, EventArgs e)
        {
            try
            {
                // Пробую преобразовать аргумент функции (агрумент события TCP-клиента) в аргумент события Адама
                xClient.ClientEventArgs client_args = e as xClient.ClientEventArgs;
                if (client_args == null) return;
                AdamEventArgs adam_args = new AdamEventArgs()
                {
                    Buffer = client_args.Buffer,
                    Message = client_args.Message,
                    State = client_args.State,
                    NewDataArrived = client_args.Message == "New data arrived" && client_args.Buffer.Length > 13
                };
                // Парсю байты в ushort значения
                ParseBytes(adam_args.Buffer);
                // Ретранслирую событие во вне
                if (OnEvent != null) OnEvent(this, adam_args);
            }
            catch (Exception ex) { }
        }
        // *******************************************************************************************************************************
        #endregion

        // *******************************************************************************************************************************
        /// <summary>
        /// Структура значений каналов
        /// </summary>
        public class ChannelsData
        {
            public enum SlotType { DIO, AIO };  // Тип канала (DIO Дискретный - AIO Аналоговый)
            BitArray[] DIO;                     // Массив массивов состояний ДК (Дискретный Канал)
            ushort[][] AIO;                     // Массив массивов значений АК  (Аналоговый Канал)

            /// <summary>
            /// Инициализация
            /// </summary>
            public ChannelsData()
            {
                // Инициализация массивов
                DIO = new BitArray[8];
                AIO = new ushort[8][];
                for (int i = 0; i < 8; i++)
                    AIO[i] = new ushort[8];
            }
            /// <summary>
            /// Обновление состояний ДК
            /// </summary>
            /// <param name="values">значения</param>
            /// <param name="swap_bytes">необходимо ли инвертировать пары байт</param>
            public void RefreshValues_Digital(ushort[] values, bool swap_bytes)
            {
                // Если длина массива меньше необходимой - выхожу
                if (values.Length != 8) return;
                // Инвентирую пары байт, если необходимо
                ushort[] temp = swap_bytes ? xClient.ToBigEndian(values) : values;
                // Обновляю состояния в массивах
                for (int i = 0; i < 8; i++)
                    DIO[i] = new BitArray(BitConverter.GetBytes(temp[i]));
            }
            /// <summary>
            /// Обновление значений АК
            /// </summary>
            /// <param name="values">значения</param>
            /// <param name="swap_bytes">необходимо ли инвертировать пары байт</param>
            public void RefreshValues_Analog(ushort[] values, bool swap_bytes)
            {
                // Если длина массива меньше необходимой - выхожу
                if (values.Length != 64) return;
                // Инвентирую пары байт, если необходимо
                ushort[] temp = swap_bytes ? xClient.ToBigEndian(values) : values;
                // Определяю шаг и размер фрагмента копирования
                int len = 8 * sizeof(ushort);
                // Обновляю значения в массивах, путём копирования памяти
                for (int i = 0; i < 8; i++)
                    Buffer.BlockCopy(temp, i * len, AIO[i], 0, len);
            }
            /// <summary>
            /// Получение массива состояний ДС
            /// </summary>
            /// <param name="slot">слот</param>
            /// <returns>массив состояний</returns>
            public BitArray GetSlotValues_Digital(int slot)
            {
                return DIO[slot];
            }
            /// <summary>
            /// Получение массива значений АС
            /// </summary>
            /// <param name="slot"></param>
            /// <returns>массив значений</returns>
            public ushort[] GetSlotValues_Analog(int slot)
            {
                return AIO[slot];
            }
            /// <summary>
            /// Получение состояния ДК
            /// </summary>
            /// <param name="slot">слот</param>
            /// <param name="channel">канал</param>
            /// <returns>состояние</returns>
            public bool GetChannelValue_Digital(int slot, int channel)
            {
                if (DIO[slot] == null) return false;
                return DIO[slot].Get(channel);
            }
            /// <summary>
            /// Получение значения АК
            /// </summary>
            /// <param name="slot">слот</param>
            /// <param name="channel">канал</param>
            /// <returns>значение</returns>
            public ushort GetChannelValue_Analog(int slot, int channel)
            {
                return AIO[slot][channel];
            }
        }
        // *******************************************************************************************************************************
        [Serializable]
        public class Settings
        {
            public Info[] Infos = new Info[0];
            public string IP;
            public Info GetChannel(string name)
            { return Array.Find<Info>(Infos, ch => ch.Name == name); }

            public struct Info
            {
                public string Name;
                public int Slot;
                public int Channel;
            }
        }
    }
}
