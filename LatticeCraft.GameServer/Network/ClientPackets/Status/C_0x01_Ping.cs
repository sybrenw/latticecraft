using Lattice.Core.Network;
using LatticeCraft.GameServer.Buffers;
using LatticeCraft.GameServer.Network.ServerPackets.Status;
using System.Buffers;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer.Network.ClientPackets.Status
{
    public class C_0x01_Ping : ReceivablePacket
    {
        public static int PacketId { get; } = 0x01;

        public long PayLoad { get; private set; }

        public override void Read(ref SequenceReader<byte> reader)
        {
            PayLoad = reader.ReadLong();
        }

        public override async Task ProcessAsync()
        {
            await Client.SendPacketAsync(new S_0x01_Pong(PayLoad));
        }
    }
}
