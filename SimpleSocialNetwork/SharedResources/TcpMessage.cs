using System;
using System.Collections.Generic;

namespace SharedResources
{
    /// <summary>A serializable structure of data, to be sent over TCP.</summary>
    [Serializable]
    public class ClientMsg
    {
        /// <summary>The type of the msg.</summary>
        public int type { get; set; }

        /// <summary>Data to be parsed on receiver side.</summary>
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

    [Serializable]
    public class ServerMsg
    {
        /// <summary>The type of the msg.</summary>
        public int type { get; set; }

        /// <summary>Data to be parsed on receiver side.</summary>
        public string user { get; set; }

        /// <summary>Data to be parsed on receiver side.</summary>
        public object data { get; set; }

        /// <summary>Default constructor; initialize a new message: id = invalid, type = invalid</summary>
        public ServerMsg()
        {
            type = TcpConst.INVALID;
            user = null;
            data = null;
        }
    }
}
