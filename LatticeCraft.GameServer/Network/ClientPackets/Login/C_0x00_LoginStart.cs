using Lattice.Core.Network;
using LatticeCraft.GameServer.Buffers;
using LatticeCraft.GameServer.Network.ServerPackets.Login;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer.Network.ClientPackets.Login
{
    public class C0x00_LoginStart : ReceivablePacket
    {
        public static int PacketId { get; } = 0x00;

        public string Username { get; private set; }

        public override void Read(ref SequenceReader<byte> reader)
        {
            Username = reader.ReadString();
        }

        public override async Task ProcessAsync()
        {
            var client = Client as MinecraftClient;
            client.Username = Username;

            await Client.SendPacketAsync(new S_0x01_EncryptionRequest("", client.KeyPair.SubjectPublicKey, client.VerifyToken));
        }
    }
}
