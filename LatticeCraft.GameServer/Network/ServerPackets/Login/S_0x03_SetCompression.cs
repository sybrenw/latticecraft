using Lattice.Core.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Login
{
    public class S_0x03_SetCompression : SendablePacket
    {
        public override int PacketId { get { return 0x03; } }

        public int ThresholdSize { get; }

        public S_0x03_SetCompression(int thresholdSize)
        {
            ThresholdSize = thresholdSize;
        }

        protected override void Write()
        {
            WriteVarInt(ThresholdSize);
        }

    }

}
