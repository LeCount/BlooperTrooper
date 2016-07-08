using System;
using System.Collections.Generic;

namespace SharedResources
{
    /// <summary>Message to be sent from client, and received on server</summary>
    [Serializable]
    public class ClientMsg
    {
        /// <summary>The type of the msg.</summary>
        public int type { get; set; }

        /// <summary>The name of the user, that this message regards.</summary>
        public string user { get; set; }

        /// <summary>Data to be parsed on receiver side.</summary>
        public object data { get; set; }

        /// <summary>Default constructor; initialize a new message: id = invalid, type = invalid</summary>
        public ClientMsg()
        {
            type = TcpConst.INVALID;
            user = null;
            data = null;
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
            data = null;
        }
    }
}
