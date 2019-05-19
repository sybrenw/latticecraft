using Lattice.Core.Network;
using LatticeCraft.GameServer.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer.Network.ClientPackets.Play
{
    class C_0x04_ClientSettings : ReceivablePacket
    {
        public static int PacketId { get; } = 0x04;

        public string Locale { get; private set; }
        public byte ViewDistance { get; private set; }
        public int ChatMode { get; private set; }
        public bool ChatColors { get; private set; }
        public byte DisplayedSkinParts { get; private set; }
        public int MainHand { get; private set; }


        public override void Read(ref SequenceReader<byte> reader)
        {
            Locale = reader.ReadString();
            ViewDistance = reader.ReadByte();
            ChatMode = reader.ReadVarInt();
            ChatColors = reader.ReadBoolean();
            DisplayedSkinParts = reader.ReadByte();
            MainHand = reader.ReadVarInt();
        }

        public override async Task ProcessAsync()
        {

        }
    }
}
