using Lattice.Core.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LatticeCraft.GameServer.Network.ServerPackets.Login
{
    public class S_0x01_EncryptionRequest : SendablePacket
    {
        public override int PacketId { get { return 0x01; } }

        public string ServerId { get; }

        public byte[] PublicKey { get; }

        public byte[] VerifyToken { get; }

        public S_0x01_EncryptionRequest(string serverId, byte[] publicKey, byte[] verifyToken)
        {
            ServerId = serverId;
            PublicKey = publicKey;
            VerifyToken = verifyToken;
        }

        protected override void Write()
        {
            WriteString(ServerId);

            WriteVarInt(PublicKey.Length);
            WriteBytes(PublicKey);

            WriteVarInt(VerifyToken.Length);
            WriteBytes(VerifyToken);
        }

    }

}
