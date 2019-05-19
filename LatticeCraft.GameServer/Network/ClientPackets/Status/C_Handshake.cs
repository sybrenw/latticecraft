using Lattice.Core.Network;
using LatticeCraft.GameServer.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer.Network.ClientPackets.Status
{
    public class C_Handshake : ReceivablePacket
    {
        public int Protocol { get; private set; }
        public string Address { get; private set; }
        public short Port { get; private set; }
        public int NextState { get; private set; }

        public override void Read(ref SequenceReader<byte> reader)
        {
            Protocol = reader.ReadVarInt();
            Address = reader.ReadString();
            Port = reader.ReadShort();
            NextState = reader.ReadVarInt();
        }

        public override Task ProcessAsync()
        {
            var client = Client as MinecraftClient;
            if (NextState == 1)
                client.ConnectionState = ConnectionState.Status;
            else
                client.ConnectionState = ConnectionState.Unauthenticated;

            return Task.CompletedTask;
        }
    }
}
