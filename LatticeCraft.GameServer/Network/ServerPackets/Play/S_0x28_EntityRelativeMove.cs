using Lattice.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Play
{
    class S_0x28_EntityRelativeMove : SendablePacket
    {
        public override int PacketId { get { return 0x28; } }

        public Entity.Entity Entity { get; }

        public S_0x28_EntityRelativeMove(Entity.Entity entity)
        {
            Entity = entity;
        }

        protected override void Write()
        {
            WriteVarInt(Entity.Id);
            WriteShort(0);
            WriteShort(0);
            WriteShort(0);
            WriteBoolean(Entity.OnGround);
        }

    }
}
