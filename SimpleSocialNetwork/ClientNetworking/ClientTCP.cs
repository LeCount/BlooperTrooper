﻿
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
        private bool server_alive = false;
        private bool connected = false;
        private static Serializer s = new Serializer();

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
                    client_stream = c.GetStream();
                    connected = true;
                }

                catch (Exception) { }
            }

            return client_stream;
        }

        public void ServerStatusPing(Stream client_stream)
        {   
            while (true)
            {
                ClientMsg msg = new ClientMsg();
                Ping_data pingdata = new Ping_data();
                pingdata.message_code = TcpMessageCode.REQUEST;

                Client_send(pingdata, TcpConst.PING, client_stream);

                Thread.Sleep(10000);

                if (!server_alive)
                {
                    connected = false;
                    //logger.Error("Ping failed!");
                }

            }
        }


        /// <summary>Send message from client to server over TCP.</summary>
        /// <param name="msg">Message to be sent over TCP.</param>
        public void Client_send(object msg_data, int msg_type, Stream client_stream)
        {
            ClientMsg msg = new ClientMsg();
            msg.type = msg_type;
            msg.data = msg_data;

            byte[] byteBuffer = s.SerializeClientMsg(msg);
            try { client_stream.Write(byteBuffer, 0, byteBuffer.Length); }
            catch (Exception) { }
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
                        message_list.Enqueue(msg);
                    }

                    numOfBytesRead = 0;
                }
            }
        }

        public void SetServerStatus(bool b)
        {
            server_alive = b;
        }

        public ServerMsg GetNextMessage()
        {
            ServerMsg msg = null;

            if (message_list.Count > 0 && message_list != null)
            {
                msg = message_list.Dequeue();
            }

            return msg;
        }
    }
}
