using Lattice.Core.Network;
using LatticeCraft.GameServer.Buffers;
using LatticeCraft.GameServer.Network.ServerPackets.Login;
using LatticeCraft.GameServer.Network.ServerPackets.Status;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer.Network.ClientPackets.Login
{
    public class C_0x01_EncryptionResponse : ReceivablePacket
    {
        public const string REQUEST_URL = "https://sessionserver.mojang.com/session/minecraft/hasJoined?username={0}&serverId={1}";

        public static int PacketId { get; } = 0x01;
        
        public byte[] RawSharedSecret { get; private set; }
        public byte[] RawVerifyToken { get; private set; }

        public override void Read(ref SequenceReader<byte> reader)
        {
            int len = reader.ReadVarInt();
            RawSharedSecret = reader.ReadBytes(len);

            len = reader.ReadVarInt();
            RawVerifyToken = reader.ReadBytes(len);
        }

        public override async Task ProcessAsync()
        {
            var client = Client as MinecraftClient;
                      
            var sharedSecret = client.KeyPair.Decrypt(RawSharedSecret);
            var verifyToken = client.KeyPair.Decrypt(RawVerifyToken);

            for (int i = 0; i < client.VerifyToken.Length; i++)
            {
                if (verifyToken[i] != client.VerifyToken[i])
                    await Client.SendPacketAsync(new S_0x00_Disconnect("Invalid verify token!"));
            }

            client.SetEncryption(sharedSecret);
                
            var preHash = sharedSecret.Concat(client.KeyPair.SubjectPublicKey).ToArray();
            var hash = CreateJavaHash(preHash);

            using (HttpClient httpClient = new HttpClient())
            {
                string uri = string.Format(REQUEST_URL, client.Username, hash);
                var result = await httpClient.GetAsync(uri);

                if (result.IsSuccessStatusCode)
                {
                    var stream = await result.Content.ReadAsStreamAsync();
                    MojangResponse obj = null;

                    using (var sr = new StreamReader(stream))
                    using (var jr = new JsonTextReader(sr))
                    {
                        obj = new JsonSerializer().Deserialize<MojangResponse>(jr);
                    }

                    if (obj != null && !string.IsNullOrEmpty(obj.Id))
                    {
                        var guid = Guid.Parse(obj.Id);
                        string uuid = guid.ToString();
                        client.ConnectionState = ConnectionState.Authenticated;
                        await client.SendPacketAsync(new S_0x02_LoginSuccess(uuid, client.Username));
                        await client.SendJoinWorld(guid);
                        return;
                    }
                }
            }

            await Client.SendPacketAsync(new S_0x00_Disconnect("Failed to verify session with mojang"));

        }

        private string CreateJavaHash(byte[] input)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();

            var hash = sha1.ComputeHash(input);
            
            BigInteger value = new BigInteger(hash, isBigEndian:true);
            if (value < 0)
            {
                return "-" + (-value).ToString("X").ToLower().TrimStart('0');
            }

            return value.ToString("X").ToLower().TrimStart('0');
        }

        private class MojangResponse
        {
            public string Id { get; set; }
        }
    }
}
