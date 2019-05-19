using Lattice.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Play
{
    public class S_0x32_PlayerPositionAndLook : SendablePacket
    {
        public override int PacketId { get { return 0x32; } }
        
        public S_0x32_PlayerPositionAndLook()
        {

        }

        protected override void Write()
        {
            WriteDouble(0);
            WriteDouble(16);
            WriteDouble(0);

            WriteFloat(0);
            WriteFloat(0);
            WriteByte(31);
            WriteVarInt(0);
        }

    }
}
