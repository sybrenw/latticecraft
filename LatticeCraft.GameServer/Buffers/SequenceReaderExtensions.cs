using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.Buffers
{
    internal static class SequenceReaderExtensions
    {
        public static bool TryReadVarInt(this ref SequenceReader<byte> reader, out int value)
        {
            value = 0;
            for (int idx = 0; idx < 5; idx++)
            {
                if (!reader.TryRead(out byte b))
                    return false;

                value |= (b & 0b01111111) << (7 * idx);

                if ((b & 0b10000000) == 0)
                    break;
            }

            return true;
        }

        public static bool TryReadVarLong(this ref SequenceReader<byte> reader, out long value)
        {
            value = 0;
            for (int idx = 0; idx < 9; idx++)
            {
                if (!reader.TryRead(out byte b))
                    return false;

                value |= ((long)b & 0b01111111) << (7 * idx);

                if ((b & 0b10000000) == 0)
                    break;
            }

            return true;
        }

        public static bool TryReadString(this ref SequenceReader<byte> reader, out string value)
        {
            value = default;

            if (!reader.TryReadVarInt(out int strLen))
                return false;

            var span = reader.UnreadSpan;
            if (span.Length >= strLen)
            {
                value = Encoding.UTF8.GetString(span.Slice(0, strLen));
                reader.Advance(strLen);
                return true;
            }

            byte[] bytes = new byte[strLen];
            for (int i = 0; i < strLen; i++)
            {
                if (!reader.TryRead(out bytes[i]))
                    return false;
            }

            value = Encoding.UTF8.GetString(bytes);
            return true;
        }

        public static int ReadVarInt(this ref SequenceReader<byte> reader)
        {
            int value = 0;
            if (reader.TryReadVarInt(out value))
                return value;

            return 0;
        }

        public static long ReadVarLong(this ref SequenceReader<byte> reader)
        {
            long value = 0;
            if (TryReadVarLong(ref reader, out value))
                return value;

            return 0;
        }

        public static short ReadShort(this ref SequenceReader<byte> reader)
        {
            short value = 0;
            if (reader.TryReadBigEndian(out value))
                return value;

            return 0;
        }

        public static int ReadInt(this ref SequenceReader<byte> reader)
        {
            int value;
            if (reader.TryReadBigEndian(out value))
                return value;

            return 0;
        }

        public static long ReadLong(this ref SequenceReader<byte> reader)
        {
            long value;
            if (reader.TryReadBigEndian(out value))
                return value;

            return 0;
        }

        public static double ReadDouble(this ref SequenceReader<byte> reader)
        {
            byte[] bytes = reader.ReadBytes(8);
            return BitConverter.ToDouble(bytes);
        }

        public static float ReadFloat(this ref SequenceReader<byte> reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            return BitConverter.ToSingle(bytes);
        }


        public static string ReadString(this ref SequenceReader<byte> reader)
        {
            string value;
            if (reader.TryReadString(out value))
                return value;

            return null;
        }

        public static byte ReadByte(this ref SequenceReader<byte> reader)
        {
            reader.TryRead(out byte b);
            return b;
        }
        
        public static bool ReadBoolean(this ref SequenceReader<byte> reader)
        {
            return reader.ReadByte() != 0;
        }

        public static byte[] ReadBytes(this ref SequenceReader<byte> reader, int amount)
        {
            var slice = reader.Sequence.Slice(reader.Position, amount);
            reader.Advance(amount);
            return slice.ToArray();
        }
    }
}
