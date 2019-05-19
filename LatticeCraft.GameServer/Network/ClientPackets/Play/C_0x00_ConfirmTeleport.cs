using Lattice.Core.Network;
using LatticeCraft.GameServer.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer.Network.ClientPackets.Play
{
    class C_0x00_ConfirmTeleport : ReceivablePacket
    {
        public static int PacketId { get; } = 0x00;

        public int TeleportId { get; private set; }

        public override void Read(ref SequenceReader<byte> reader)
        {
            TeleportId = reader.ReadVarInt();
        }
    }
}
