using Lattice.Core.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Status
{
    public class S_0x01_Pong : SendablePacket
    {
        public override int PacketId { get { return 0x01; } }

        public long PayLoad { get; }

        public S_0x01_Pong(long payload)
        {
            PayLoad = payload;
        }

        protected override void Write()
        {
            WriteLong(PayLoad);
        }

    }

}
