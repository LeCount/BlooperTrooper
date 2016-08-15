
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using SharedResources;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace ClientNetworking
{
    public class ClientTCP
    {
        private AppLogger log = new AppLogger();

        private Queue<ServerMsg> message_list = new Queue<ServerMsg>();
        private bool connected = false;
        private static Serializer s = new Serializer();
        private object message_list_lock = new object();

        /// <summary>A byte-array based buffer, where incoming messages are stored.</summary>
        private byte[] receive_buffer = new byte[TcpConst.BUFFER_SIZE];

        /// <summary>Try to connect to server.</summary>
        /// <param name="c">TCP client</param>
        /// <param name="addr">IP address to server</param>
        /// <param name="port">Server port</param>
        /// <returns>A network stream to reveive and send data on.</returns>
        public Stream ConnectToServer(TcpClient c, string server_addr, int port)
        {
            connected = false;
            Stream client_stream = null;

            while (!connected)
            {
                try
                {
                    c.Connect(IPAddress.Parse(server_addr), port);
                    //c.Connect(IPAddress.Parse("193.11.112.234"), port);
                    client_stream = c.GetStream();
                    connected = true;
                }

                catch (Exception) { }
            }

            return client_stream;
        }


        /// <summary>Send message from client to server over TCP.</summary>
        /// <param name="msg">Message to be sent over TCP.</param>
        public bool Client_send(object msg_data, int msg_type, Stream client_stream)
        {
            ClientMsg msg = new ClientMsg();
            msg.type = msg_type;
            msg.data = msg_data;

            byte[] byteBuffer = s.SerializeClientMsg(msg);
            try
            {
                client_stream.Write(byteBuffer, 0, byteBuffer.Length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Read messages from the server</summary>
        public void ClientRead(Stream client_stream)
        {
            int numOfBytesRead = 0;

            while (true)
            {
                try
                {
                    numOfBytesRead = client_stream.Read(receive_buffer, 0, TcpConst.BUFFER_SIZE);
                }
                catch (Exception) { }

                if (numOfBytesRead > 0)
                {
                    ServerMsg msg = s.DeserializeServerMsg(receive_buffer);

                    if (msg != null)
                    {
                        EnqueueWithMutex(msg);
                    }

                    numOfBytesRead = 0;
                }
            }
        }

        private ServerMsg DequeueWithMutex()
        {
            lock(message_list_lock)
                return message_list.Dequeue();
        }

        private void EnqueueWithMutex(ServerMsg msg)
        {
            lock (message_list_lock)
                message_list.Enqueue(msg);
        }

        public ServerMsg GetNextMessage()
        {
            ServerMsg msg = null;

            if (message_list.Count > 0 && message_list != null)
            {
                msg = DequeueWithMutex();
            }

            return msg;
        }
    }
}
