using Lattice.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Play
{
    public class S_0x49_SpawnPosition : SendablePacket
    {
        public override int PacketId { get { return 0x49; } }
        
        public S_0x49_SpawnPosition()
        {

        }

        protected override void Write()
        {
            long x = 0;
            long y = 16;
            long z = 0;

            long pos = ((x & 0x3FFFFFF) << 38) | ((y & 0xFFF) << 26) | (z & 0x3FFFFFF);

            WriteLong(pos);
        }

    }
}
