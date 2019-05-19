using Lattice.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Play
{
    class S_0x25_JoinGame : SendablePacket
    {
        public override int PacketId { get { return 0x25; } }
        
        public S_0x25_JoinGame()
        {

        }

        protected override void Write()
        {
            WriteInt(1);
            WriteByte(0);
            WriteInt(0);
            WriteByte(3);
            WriteByte(255);
            WriteString("flat");
            WriteByte(0);
        }

    }
}
