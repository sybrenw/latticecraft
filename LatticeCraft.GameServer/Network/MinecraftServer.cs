using Lattice.Core.Network;
using Lattice.Core.Network.Pipelines;
using LatticeCraft.GameServer.Crypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace LatticeCraft.GameServer.Network
{
    internal class MinecraftServer : NetworkListener
    {
        private PacketSelector _selector;

        private KeyPair _keyPair;

        private Random _random;

        public MinecraftServer()
        {
            _selector = new PacketSelector();
            _keyPair = new KeyPair();
        }

        protected override IConnection CreateConnection(Socket socket, CancellationToken disconnectToken)
        {
            var pipe = new DxEncryptPipe(disconnectToken);

            return new MinecraftClient(socket, pipe, disconnectToken, _selector, _keyPair);
        }
    }
}
