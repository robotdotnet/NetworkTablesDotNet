using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

namespace NetworkTables.NetworkTables2.Connection
{


    public class DataIOStream
    { 
        private NetworkStream stream;

        public DataIOStream(NetworkStream stream)
        {
            this.stream = stream;
        }

        public void WriteByte(byte b)
        {
            stream.WriteByte(b);
        }

        public void WriteCharBE(char s)
        {
            WriteByte((byte)(s >> 8));
            WriteByte((byte)s);
        }

        public void WriteString(string str)
        {
            WriteCharBE((char)str.Length);
            byte[] value = System.Text.Encoding.UTF8.GetBytes(str);
            stream.Write(value, 0, value.Length);
        }

        public void Flush()
        {
            stream.Flush();
        }

        public byte ReadByte()
        {
            return (byte)stream.ReadByte();
        }

        public char ReadCharBE()
        {
            return (char)(stream.ReadByte() << 8 | stream.ReadByte());
        }

        public string ReadString()
        {
            char byteLength = ReadCharBE();
            byte[] bytes = new byte[byteLength];
            stream.Read(bytes, 0, byteLength);
            return Encoding.UTF8.GetString(bytes);
        }

        public void Close()
        {
            stream.Close();
        }
    }
}
