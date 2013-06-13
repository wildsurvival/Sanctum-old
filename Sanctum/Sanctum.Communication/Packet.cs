using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanctum.Communication
{
    public enum PacketFamily : byte
    {
        Initialize = 0
    }

    public enum PacketAction : byte
    {
        Initialize = 0
    }

    public class Packet
    {
        private List<byte> Data;

        private int ReadPosition = 6;
        private int WritePosition = 6;

        public PacketFamily Family { get { return (PacketFamily)Data[0]; } set { Data[0] = (byte)value; } }
        public PacketAction Action { get { return (PacketAction)Data[1]; } set { Data[1] = (byte)value; } }

        public int Length { get { return Data.Count; } }

        public const byte Break = 0xFF;

        public Packet(byte[] data)
        {
            this.Data = new List<byte>(data);
        }

        public Packet(PacketFamily family, PacketAction action)
        {
            Data = new List<byte>(2);
            Data.Add((byte)family);
            Data.Add((byte)action);
            Data.AddRange(new byte[] { 0, 0, 0, 0 }); //Checksum
        }

        public void SetID(PacketFamily family, PacketAction action)
        {
            Data[0] = (byte)family;
            Data[1] = (byte)action;
        }

        public void Write<T>(object data)
        {
            Type type = typeof(T);

            if (type == typeof(byte)) //Byte or Char
            {
                byte byteValue = (byte)data;

                Data.Insert(WritePosition, byteValue);
                WritePosition += 1;
            }
            else if (type == typeof(byte[])) //Byte[]
            {
                byte[] byteValues = (byte[])data;

                Data.InsertRange(WritePosition, byteValues);
                WritePosition += byteValues.Length;
            }
            else if (type == typeof(string)) //String
            {
                byte[] byteValues = ASCIIEncoding.ASCII.GetBytes((string)data);

                Data.InsertRange(WritePosition, byteValues);
                WritePosition += byteValues.Length;
            }
            else if (type == typeof(short)) //Short
            {
                short shortValue = (short)data;

                byte[] bytes = new byte[2] { (byte)(shortValue >> 8), (byte)shortValue };
                Data.InsertRange(WritePosition, bytes);
                WritePosition += 2;
            }
            else if (type == typeof(int)) //Integer
            {
                int intValue = (int)data;

                byte[] bytes = new byte[4] {(byte)(intValue >> 24), (byte)(intValue >> 16), (byte)(intValue >> 8), (byte)intValue};
                Data.InsertRange(WritePosition, bytes);
                WritePosition += 4;
            }
        }

        public T Read<T>(bool peek = false)
        {
            Type type = typeof(T);
            object data = null;

            if (type == typeof(byte)) //Byte or Char
            {
                data = Data.GetRange(ReadPosition, 1)[0];

                if (!peek)
                    ReadPosition += 1;
            }
            else if (type == typeof(short)) //Short
            {
                List<byte> bytes = Data.GetRange(ReadPosition, 2);

                data = bytes[0] << 8 | bytes[1];

                if (!peek)
                    ReadPosition += 2;
            }
            else if (type == typeof(int)) //Integer
            {
                List<byte> bytes = Data.GetRange(ReadPosition, 4);

                data = bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];

                if (!peek)
                    ReadPosition += 4;
            }

            return (T)data;
        }

        public byte[] ReadBytes(int length, bool peek = false)
        {
            byte[] bytes = Data.GetRange(ReadPosition, length).ToArray();

            if (!peek)
                ReadPosition += length;

            return bytes;
        }

        public string ReadString(int length, bool peek = false)
        {
            byte[] bytes = ReadBytes(length, peek);
            string data = ASCIIEncoding.ASCII.GetString(bytes);

            return data;
        }

        public void Clear()
        {
            PacketFamily family = Family;
            PacketAction action = Action;

            Data.Clear();
            this.ReadPosition = 6;
            this.WritePosition = 6;

            Data.Add((byte)family);
            Data.Add((byte)action);
            Data.AddRange(new byte[] { 0, 0, 0, 0 }); //Checksum
        }
    }
}
