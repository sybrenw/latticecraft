using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lattice.Core.Network
{
    public abstract class SendablePacket
    {
        public abstract int PacketId { get; }

        public NetworkConnection Client { get; set; }
        
        protected abstract void Write();

        private Stream _stream;
        private byte[] _buffer;

        protected long Position => _stream.Position;

        public ReadOnlyMemory<byte> Header { get; private set; }

        public void WritePacket(Stream stream)
        {
            _buffer = new byte[16];
            _stream = stream;
            WriteVarInt(PacketId);
            Write();
            
            // Reuse the buffer to create the header
            int len = WriteVarInt(_buffer, 0, (int)_stream.Position);
            Header = new ReadOnlyMemory<byte>(_buffer, 0, len);
        }
        
        protected int WriteVarInt(byte[] buffer, int offset, int value)
        {
            byte tmp;
            int idx = 0;

            do
            {
                tmp = (byte)(value & 0b01111111);
                value >>= 7;
                if (value != 0)
                    tmp |= 0b10000000;

                buffer[offset + idx] = tmp;
                idx++;
            } while (value != 0);

            return idx;
        }
        protected void WriteBoolean(bool value)
        {
            WriteByte(value ? (byte)1 : (byte)0);
        }

        protected void WriteByte(byte b)
        {
            _buffer[0] = b;
            _stream.Write(_buffer, 0, 1);
        }

        protected void WriteBytes(byte[] bytes)
        {
            _stream.Write(bytes);
        }

        protected void WriteShort(short value)
        {
            _buffer[0] = (byte)(value >> 8);
            _buffer[1] = (byte)value;
            _stream.Write(_buffer, 0, 2);
        }

        protected void WriteInt(int value)
        {
            _buffer[0] = (byte)(value >> 24);
            _buffer[1] = (byte)(value >> 16);
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)value;
            _stream.Write(_buffer, 0, 4);
        }
        
        protected void WriteLong(long value)
        {
            _buffer[0] = (byte)(value >> 56);
            _buffer[1] = (byte)(value >> 48);
            _buffer[2] = (byte)(value >> 40);
            _buffer[3] = (byte)(value >> 32);
            _buffer[4] = (byte)(value >> 24);
            _buffer[5] = (byte)(value >> 16);
            _buffer[6] = (byte)(value >> 8);
            _buffer[7] = (byte)value;
            _stream.Write(_buffer, 0, 8);
        }

        protected void WriteLong(ulong value)
        {
            _buffer[0] = (byte)(value >> 56);
            _buffer[1] = (byte)(value >> 48);
            _buffer[2] = (byte)(value >> 40);
            _buffer[3] = (byte)(value >> 32);
            _buffer[4] = (byte)(value >> 24);
            _buffer[5] = (byte)(value >> 16);
            _buffer[6] = (byte)(value >> 8);
            _buffer[7] = (byte)value;
            _stream.Write(_buffer, 0, 8);
        }

        protected void WriteULong(ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            _stream.Write(bytes, 0, 8);
        }

        protected void WriteFloat(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            _stream.Write(bytes, 0, bytes.Length);
        }

        protected void WriteDouble(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            _stream.Write(bytes, 0, bytes.Length);
        }

        protected void WriteVarInt(int value)
        {
            byte tmp;
            
            do
            {
                tmp = (byte)(value & 0b01111111);
                // Note: >>> means that the sign bit is shifted with the rest of the number rather than being left alone
                value >>= 7;
                if (value != 0)
                    tmp |= 0b10000000;

                _stream.WriteByte(tmp);
            } while (value != 0);
        }

        protected void WriteString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteVarInt(bytes.Length);
            _stream.Write(bytes);
        }

    }
}
