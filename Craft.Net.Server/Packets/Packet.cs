using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Craft.Net.Server.Packets
{
    public enum PacketContext
    {
        ClientToServer,
        ServerToClient
    }

    public abstract class Packet
    {
        public PacketContext PacketContext { get; set; }
        public abstract byte PacketID { get; }

        public event EventHandler OnPacketSent;

        public void FirePacketSent()
        {
            if (OnPacketSent != null)
                OnPacketSent(this, null);
        }

        public abstract int TryReadPacket(byte[] buffer, int length);
        public abstract void HandlePacket(MinecraftServer server, ref MinecraftClient client);
        public abstract void SendPacket(MinecraftServer server, MinecraftClient client);

        public override string ToString()
        {
            Type type = GetType();
            string value = type.Name + " (0x" + PacketID.ToString("x") + ")\n";
            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo field in fields)
            {
                value += "\t" + field.Name + ": " + field.GetValue(this) + "\n";
            }
            return value.Remove(value.Length - 1);
        }

        #region Packet Writer Methods

        protected internal static byte[] CreateString(string text)
        {
            if (text.Length > 240)
                throw new ArgumentOutOfRangeException("text", "String length cannot be greater 240 characters.");

            return CreateShort((short)text.Length)
                .Concat(Encoding.BigEndianUnicode.GetBytes(text)).ToArray();
        }

        protected internal static byte[] CreateBoolean()
        {
            return CreateBoolean(false);
        }

        protected internal static byte[] CreateBoolean(bool value)
        {
            return new[]
                       {
                           unchecked((byte)(value ? 1 : 0))
                       };
        }

        protected internal static byte[] CreateUShort(ushort value)
        {
            unchecked
            {
                return new[] {(byte)(value >> 8), (byte)(value)};
            }
        }

        protected internal static byte[] CreateShort(short value)
        {
            unchecked
            {
                return new[] {(byte)(value >> 8), (byte)(value)};
            }
        }

        protected internal static byte[] CreateInt(int value)
        {
            unchecked
            {
                return new[]
                           {
                               (byte)(value >> 24), (byte)(value >> 16),
                               (byte)(value >> 8), (byte)(value)
                           };
            }
        }

        protected internal static byte[] CreateLong(long value)
        {
            unchecked
            {
                return new[]
                           {
                               (byte)(value >> 56), (byte)(value >> 48),
                               (byte)(value >> 40), (byte)(value >> 32),
                               (byte)(value >> 24), (byte)(value >> 16),
                               (byte)(value >> 8), (byte)(value)
                           };
            }
        }

        protected internal static unsafe byte[] CreateFloat(float value)
        {
            return CreateInt(*(int*)&value);
        }

        protected internal static byte[] CreatePackedByte(float value)
        {
            return new[] {(byte)(((Math.Floor(value)%360)/360)*256)};
        }

        protected internal static unsafe byte[] CreateDouble(double value)
        {
            return CreateLong(*(long*)&value);
        }

        #endregion

        #region Packet Reader Methods

        protected static short ReadShort(byte[] buffer, int offset)
        {
            return unchecked((short)(buffer[0 + offset] << 8 | buffer[1 + offset]));
        }

        protected internal static bool TryReadBoolean(byte[] buffer, ref int offset, out bool value)
        {
            value = false;
            if (buffer.Length - offset >= 1)
            {
                value = buffer[offset++] == 1;
                return true;
            }
            return false;
        }

        protected internal static bool TryReadByte(byte[] buffer, ref int offset, out byte value)
        {
            value = 0;
            if (buffer.Length - offset >= 1)
            {
                value = buffer[offset++];
                return true;
            }
            return false;
        }

        protected internal static bool TryReadShort(byte[] buffer, ref int offset, out short value)
        {
            value = -1;
            if (buffer.Length - offset >= 2)
            {
                value = unchecked((short)(buffer[0 + offset] << 8 | buffer[1 + offset]));
                offset += 2;
                return true;
            }
            return false;
        }

        protected internal static bool TryReadInt(byte[] buffer, ref int offset, out int value)
        {
            value = -1;
            if (buffer.Length - offset >= 4)
            {
                value = unchecked(buffer[0 + offset] << 24 |
                                  buffer[1 + offset] << 16 |
                                  buffer[2 + offset] << 8 |
                                  buffer[3 + offset]);
                offset += 4;
                return true;
            }
            return false;
        }

        protected internal static bool TryReadLong(byte[] buffer, ref int offset, out long value)
        {
            if (buffer.Length - offset >= 4)
            {
                unchecked
                {
                    value = 0;
                    value |= (long)buffer[0 + offset] << 56;
                    value |= (long)buffer[1 + offset] << 48;
                    value |= (long)buffer[2 + offset] << 40;
                    value |= (long)buffer[3 + offset] << 32;
                    value |= (long)buffer[4 + offset] << 24;
                    value |= (long)buffer[5 + offset] << 16;
                    value |= (long)buffer[6 + offset] << 8;
                    value |= buffer[7 + offset];
                }
                offset += 8;
                return true;
            }
            value = -1;
            return false;
        }

        protected internal static unsafe bool TryReadFloat(byte[] buffer, ref int offset, out float value)
        {
            value = -1;
            int i;

            if (TryReadInt(buffer, ref offset, out i))
            {
                value = *(float*)&i;
                return true;
            }

            return false;
        }

        protected internal static unsafe bool TryReadDouble(byte[] buffer, ref int offset, out double value)
        {
            value = -1;
            long l;

            if (TryReadLong(buffer, ref offset, out l))
            {
                value = *(double*)&l;
                return true;
            }

            return false;
        }

        protected internal static bool TryReadString(byte[] buffer, ref int offset, out string value)
        {
            value = null;
            short length;

            if (buffer.Length - offset >= 2)
                length = ReadShort(buffer, offset);
            else
                return false;

            if (length < 0)
                throw new ArgumentOutOfRangeException("value", "String length is less than zero");

            offset += 2;
            if (buffer.Length - offset >= length)
            {
                value = Encoding.BigEndianUnicode.GetString(buffer, offset, length*2);
                offset += length*2;
                return true;
            }

            return false;
        }

        protected internal static bool TryReadArray(byte[] buffer, short length, ref int offset, out byte[] value)
        {
            value = null;
            if (buffer.Length - offset < length)
                return false;
            value = new byte[length];
            Array.Copy(buffer, offset, value, 0, length);
            offset += length;
            return true;
        }

        #endregion
    }
}