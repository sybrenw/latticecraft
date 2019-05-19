using Lattice.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Play
{
    class S_0x27_EntityInactive : SendablePacket
    {
        public override int PacketId { get { return 0x27; } }

        public int EntityId { get; }

        public S_0x27_EntityInactive(int entityId)
        {
            EntityId = entityId;
        }

        protected override void Write()
        {
            WriteVarInt(EntityId);
        }

    }
}
