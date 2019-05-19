using Lattice.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Play
{
    public class S_0x00_SpawnObject : SendablePacket
    {
        public override int PacketId { get { return 0x00; } }

        public string Reason { get; }

        public S_0x00_SpawnObject(string reason)
        {
            Reason = reason;
        }

        protected override void Write()
        {

        }

    }
}
