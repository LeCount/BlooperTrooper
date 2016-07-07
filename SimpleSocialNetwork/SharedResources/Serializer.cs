using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static SharedResources.TcpNetworking;

namespace SharedResources
{
    /// <summary>
    /// The TCP socket between the client and the server, sends information to eachother in the form of byte-arrays. 
    /// This class is responsible for serializing and dezerializing the TcpMessage-class back and forth from byte-array format.
    /// This way, the dataexchange over TCP can be more structured and easily accessed.
    /// </summary>
    public class Serializer
    {
        MemoryStream mem_stream = null;
        BinaryFormatter bin_formater = new BinaryFormatter();

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

        public Stream ByteArrayToStream(Byte[] byte_array)
        {
            return new MemoryStream(byte_array);
        }

        public byte[] SerializeMsg(ClientMsg msg)
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

        public ClientMsg DeserializeByteArray(byte[] byte_array)
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
    }
}
