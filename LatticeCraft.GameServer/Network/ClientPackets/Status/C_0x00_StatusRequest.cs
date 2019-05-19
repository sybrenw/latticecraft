using Lattice.Core.Network;
using LatticeCraft.GameServer.Buffers;
using LatticeCraft.GameServer.Network.ServerPackets.Status;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer.Network.ClientPackets.Status
{
    public class C_0x00_StatusRequest : ReceivablePacket
    {
        public static int PacketId { get; } = 0x00;
               
        public override void Read(ref SequenceReader<byte> reader)
        {
            // Empty
        }

        public override async Task ProcessAsync()
        {
            await Client.SendPacketAsync(new S_0x00_StatusResponse());
        }
    }
}
