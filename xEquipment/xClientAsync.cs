using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace xEquipment
{
    public class xClientAsync
    {
        private Socket _socket = null;
        private IPEndPoint _remoteEP = null;
        private bool _is_connected = false;
        private const int port = 502;
        private string _error = "";

        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        public class StateObject
        {
            // Client socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 256;
            // Receive buffer.  
            public byte[] buffer = new byte[BufferSize];
            public byte[] received = new byte[0];
            // Received data string.  
            public StringBuilder sb = new StringBuilder();
        }
        public class ClientEventArgs : EventArgs
        {
            public byte[] Buffer;   // Массив байт значений
            public string Message;  // Сообщение
            public bool State;      // Флаг состояния (нафиг нужен??? мож удалить??)
        }
        public event EventHandler<ClientEventArgs> OnEvent;
        /// <summary>
        /// Аргументы события
        /// </summary>
        public bool IsConnected
        { get { return _socket != null &&  _socket.Connected; } }
        public static void Send_Recieve(IPAddress remoteIP, int port, byte[] bytes)
        {
            //// Connect to a remote device.  
            //try
            //{
            //    IPEndPoint remoteEP = new IPEndPoint(remoteIP, port);

            //    // Create a TCP/IP socket.  
            //    Socket client = new Socket(remoteIP.AddressFamily,
            //        SocketType.Stream, ProtocolType.Tcp);

            //    // Connect to the remote endpoint.  
            //    client.BeginConnect(remoteEP,
            //        new AsyncCallback(ConnectCallback), client);
            //    connectDone.WaitOne();
            //    byte[] _cmd_read_AIO = new byte[] // Команда запроса значений всех аналоговых каналов
            //    {
            //            0x00, 0x00, 0x00, 0x00, 0x00, 0x06,
            //            0x01, 0x04, 0x00, 0x00, 0x00, 0x40
            //    };

            //    // Send test data to the remote device.  
            //    Send(client, _cmd_read_AIO);
            //    sendDone.WaitOne();

            //    // Receive the response from the remote device.  
            //    Receive(client);
            //    receiveDone.WaitOne();

            //    // Write the response to the console.  
            //    Console.WriteLine("Response received : {0}", response);

            //    // Release the socket.  
            //    client.Shutdown(SocketShutdown.Both);
            //    client.Close();

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //}
        }

        public bool Connect(IPAddress remoteIP, int port)
        {
            // Connect to a remote device.  
            try
            {
                _remoteEP = new IPEndPoint(remoteIP, port);

                _socket = new Socket(remoteIP.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                if (_socket.Connected) return true;

                _socket.BeginConnect(_remoteEP, new AsyncCallback(ConnectCallback), _socket);
                connectDone.WaitOne(3000, true);

                return _socket.Connected;
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                return false;
            }
        }
        public void Disconnect()
        {
            if (!_socket.Connected) return;
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        public bool Send(byte[] bytes)
        {
            try
            {
                if (!_socket.Connected) return false;
                Send_Recieve(bytes);
                //Send(_socket, bytes);
                //sendDone.WaitOne();

                return true;
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                return false;
            }
        }
        public bool Send_Recieve(byte[] bytes)
        {
            try
            {
                if (!_socket.Connected) return false;

                Send(_socket, bytes);
                sendDone.WaitOne();

                Receive(_socket);
                receiveDone.WaitOne();

                return true;
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                return false;
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                // Signal that the connection has been made.  
                connectDone.Set();
                _is_connected = client.Connected;
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
        }
        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);
                state.received = new byte[bytesRead];
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    Array.Copy(state.buffer, 0, state.received, 0, bytesRead);

                    // Get the rest of the data.  
                    BroadcastMessage(state.received, "Received", true);
                    
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void Send(Socket client, byte[] byteData)
        {
            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                
                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
        }
        private void BroadcastMessage(byte[] Buffer, string Message, bool State)
        {
            if (OnEvent != null)
            {
                ClientEventArgs args = new ClientEventArgs();
                args.Buffer = Buffer;
                args.Message = Message;
                args.State = State;
                OnEvent(this, args);
            }
        }
    }
}
