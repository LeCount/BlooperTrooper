using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static SharedResources.TcpMethods;

namespace SharedResources
{
    /// <summary>
    /// The TCP socket between the client and the server, sends information to eachother in the form of byte-arrays. 
    /// This class is responsible for serializing and dezerializing the TcpMessage-class back and forth from byte-array format.
    /// This way, the data exchange over TCP can be more structured and easily parsed and accessed.
    /// </summary>
    public class Serializer
    {
        MemoryStream mem_stream = null;
        BinaryFormatter bin_formater = new BinaryFormatter();

        /// <summary>Converts the given stream into a byte array.</summary>
        public byte[] StreamToByteArray(Stream stream)
        {
            int bit;
            byte[] byte_array = new byte[TcpConst.BUFFER_SIZE];
            mem_stream = new MemoryStream();

            while ((bit = stream.Read(byte_array, 0, byte_array.Length)) > 0)
            {
                mem_stream.Write(byte_array, 0, bit);
            }

            return mem_stream.ToArray();
        }

        /// <summary>Converts the given byte array into a stream.</summary>
        public Stream ByteArrayToStream(Byte[] byte_array)
        {
            return new MemoryStream(byte_array);
        }

        /// <summary>Serializes a client message into a byte array.</summary>
        public byte[] SerializeClientMsg(ClientMsg msg)
        {
            mem_stream = new MemoryStream();

            try
            {
                bin_formater.Serialize(mem_stream, msg);
                return mem_stream.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Serializes a server message into a byte array.</summary>
        public byte[] SerializeServerMsg(ServerMsg msg)
        {
            mem_stream = new MemoryStream();

            try
            {
                bin_formater.Serialize(mem_stream, msg);
                return mem_stream.ToArray();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>Deserializes a byte array into a client message.</summary>
        public ClientMsg DeserializeClientMsg(byte[] byte_array)
        {
            mem_stream = new MemoryStream(byte_array);
            
            try
            {
                return (ClientMsg)bin_formater.Deserialize(mem_stream);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Deserializes a byte array into a server message.</summary>
        public ServerMsg DeserializeServerMsg(byte[] byte_array)
        {
            mem_stream = new MemoryStream(byte_array);

            try
            {
                return (ServerMsg)bin_formater.Deserialize(mem_stream);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
