using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LatticeCraft.GameServer.Crypt
{
    public class KeyPair
    {
        public byte[] PrivateKey { get; }
        public byte[] SubjectPublicKey { get; }
        public byte[] PublicKey { get; }

        private RSAParameters _parameters;

        public KeyPair()
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = 1024;

                _parameters = rsa.ExportParameters(true);
                PrivateKey = rsa.ExportRSAPrivateKey();
                SubjectPublicKey = rsa.ExportSubjectPublicKeyInfo();
                PublicKey = rsa.ExportRSAPublicKey();
            }
        }

        public byte[] Decrypt(byte[] raw)
        {
            using (RSA rsa = RSA.Create(_parameters))
            {
                return rsa.Decrypt(raw, RSAEncryptionPadding.Pkcs1);
            }
        }

    }
}
