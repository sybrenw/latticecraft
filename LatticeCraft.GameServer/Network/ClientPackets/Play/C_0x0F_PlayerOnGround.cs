using Lattice.Core.Network;
using LatticeCraft.GameServer.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer.Network.ClientPackets.Play
{
    class C_0x0F_PlayerOnGround : ReceivablePacket
    {
        public static int PacketId { get; } = 0x0F;
        
        public override void Read(ref SequenceReader<byte> reader)
        {
            var client = Client as MinecraftClient;
            client.Player.OnGround = reader.ReadBoolean();
        }
    }
}
