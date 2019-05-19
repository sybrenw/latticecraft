using Lattice.Core;
using Lattice.Core.Network;
using Lattice.Core.Network.Pipelines;
using LatticeCraft.GameServer.Buffers;
using LatticeCraft.GameServer.Crypt;
using LatticeCraft.GameServer.Entity.Player;
using LatticeCraft.GameServer.Network.ClientPackets.Status;
using LatticeCraft.GameServer.Network.ServerPackets.Play;
using LatticeCraft.GameServer.World;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer.Network
{
    internal class MinecraftClient : NetworkConnection
    {
        private static ILogger Logger { get; } = AppLogging.CreateLogger<MinecraftClient>();

        public ConnectionState ConnectionState { get; internal set; } = ConnectionState.Unknown;

        public bool CompressionEnabled { get; set; }
        public KeyPair KeyPair { get; }
        public byte[] SharedSecret { get; set; }
        public byte[] VerifyToken { get; }
        public string Username { get; set; }
        public Player Player { get; private set; }
        
        private PacketSelector _selector;

        private ICryptoTransform _encryptor;
        private ICryptoTransform _decryptor;

        private DxEncryptPipe _encryptPipe;

        public MinecraftClient(Socket socket, DxEncryptPipe encryptPipe, CancellationToken disconnectToken, PacketSelector selector, KeyPair keyPair) : base(socket, encryptPipe, disconnectToken)
        {
            _selector = selector;
            _encryptPipe = encryptPipe;

            VerifyToken = new byte[4];
            RandomUtils.Random.NextBytes(VerifyToken);

            KeyPair = keyPair;
        }

        private void Connect()
        {

        }

        protected override Task Initialize()
        {
            return Task.CompletedTask;
        }

        protected override ReceivablePacket ReadPacket(ref SequenceReader<byte> reader)
        {
            if (ConnectionState == ConnectionState.Unauthenticated && reader.TryPeek(out byte firstByte) && firstByte == 0xFE)
            {
                // Server ping
                return null;
            }

            if (!reader.TryReadVarInt(out int length))
                return null;

            long start = reader.Consumed;

            if (!reader.TryReadVarInt(out int packetId))
                return null;

            long idLength = reader.Consumed - start;

            ReceivablePacket packet = null;
            // Check handshake
            if (ConnectionState == ConnectionState.Unknown)
            {
                if (packetId != 0)
                    return null;

                packet = new C_Handshake();
            }
            else if (ConnectionState == ConnectionState.Status)
            {
                packet = _selector.SelectStatusPacket(packetId);
            }
            else if (ConnectionState == ConnectionState.Unauthenticated)
            {
                packet = _selector.SelectLoginPacket(packetId);
            }
            else
            {
                packet = _selector.SelectPlayPacket(packetId);
            }

            if (CompressionEnabled)
            {

            }

            if (packet != null)
            {
                packet.Length = length;
            }
            else
            {
                Logger.LogInformation("Skipping {0} bytes", length);
                reader.Advance(length - idLength);
            }

            return packet;
        }

        public async Task SendJoinWorld(Guid playerGuid)
        {
            Player = new Player(playerGuid);

            List<Chunk> chunks = new List<Chunk>();

            for (int ix = -3; ix <= 3; ix++)
            {
                for(int iz = -3; iz <= 3; iz++)
                {
                    chunks.Add(new Chunk() { X = ix, Z = iz });
                }
            }

            await SendPacketAsync(new S_0x25_JoinGame());

            foreach(var chunk in chunks)
                await SendPacketAsync(new S_0x22_ChunkData(chunk));

            await SendPacketAsync(new S_0x49_SpawnPosition());

            await SendPacketAsync(new S_0x32_PlayerPositionAndLook());
        }

        public void SetEncryption(byte[] key)
        {
            _encryptPipe.SetEncryption(key);
        }
    }
}
