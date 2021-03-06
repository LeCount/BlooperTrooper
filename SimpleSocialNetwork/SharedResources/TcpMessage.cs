﻿using System;

namespace SharedResources
{
    /// <summary>Message to be sent from client, and received on server</summary>
    [Serializable]
    public class ClientMsg
    {
        /// <summary>The type of the msg.</summary>
        public int type { get; set; }

        /// <summary>Data to be parsed on receiver side.</summary>
        public object data { get; set; }

        /// <summary>Default constructor; initialize a new message: id = invalid, type = invalid</summary>
        public ClientMsg()
        {
            type = TcpConst.INVALID;
            data = new Object();
        }
    }   

    /// <summary>Message to be sent from server, and received on client</summary>
    [Serializable]
    public class ServerMsg
    {
        /// <summary>The type of the msg.</summary>
        public int type { get; set; }

        /// <summary>Data to be parsed on receiver side.</summary>
        public object data { get; set; }

        /// <summary>Default constructor; initialize a new message: id = invalid, type = invalid</summary>
        public ServerMsg()
        {
            type = TcpConst.INVALID;
            data = new Object();
        }
    }
}
