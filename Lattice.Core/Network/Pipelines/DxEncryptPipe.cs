using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lattice.Core.Network.Pipelines
{
    public class DxEncryptPipe : IDxPipe, IDisposable
    {
        private static ILogger Logger { get; } = AppLogging.CreateLogger<DxEncryptPipe>();

        const int MINIMUM_BUFFER_SIZE = 512;

        /* IDuplexPipe interface */
        public PipeWriter Input => _inputPipe.Writer;
        public PipeReader Output => _outputPipe.Reader;

        public IDxPipe Source => _source;

        /* Private members */
        private Pipe _inputPipe;
        private Pipe _outputPipe;
        private IDxPipe _source;

        private int _minimumBufferSize;

        // Is the pipe open
        private bool _connected = true;

        private CancellationTokenSource _disconnectTokenSource;
        
        private bool _enabled = false;
        private byte[] _key;
        private byte[] _encryptIV;
        private byte[] _decryptIV;

        public DxEncryptPipe(CancellationToken disconnectToken = default) : this(MINIMUM_BUFFER_SIZE, disconnectToken) { }

        public DxEncryptPipe(int minimumBufferSize, CancellationToken disconnectToken = default)
        {
            _disconnectTokenSource = CancellationTokenSource.CreateLinkedTokenSource(disconnectToken);

            _minimumBufferSize = minimumBufferSize;

            // Create input and output pipe
            _inputPipe = new Pipe();
            _outputPipe = new Pipe();

            /*

            var stream = new CryptoStream(null, _encryptor, CryptoStreamMode.Write);
            */
            
        }

        public void SetEncryption(byte[] sharedSecret)
        {
            _key = sharedSecret;
            _encryptIV = sharedSecret.ToArray();
            _decryptIV = sharedSecret.ToArray();
            _enabled = true;
        }

        public void ConnectPipe(IDxPipe source)
        {
            _source = source;
        }
        
        public async Task RunAsync()
        {
            Task receiving = EncryptAsync(_inputPipe.Reader, _source.Input);
            Task sending = DecryptAsync(_source.Output, _outputPipe.Writer);
            await Task.WhenAll(receiving, sending);
        }

        private async Task EncryptAsync(PipeReader input, PipeWriter output)
        {
            while(!_disconnectTokenSource.IsCancellationRequested)
            {
                Memory<byte> memory = output.GetMemory(_minimumBufferSize);

                ReadResult result;

                try
                {
                    result = await input.ReadAsync(_disconnectTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to send message: {0}", ex);
                    break;
                }

                if (result.IsCanceled || result.IsCompleted)
                    break;

                ReadOnlySequence<byte> buffer = result.Buffer;

                // Check if encryption is enabled, else just pass through data
                if (!_enabled)
                {
                    foreach (var segment in buffer)
                        await output.WriteAsync(segment);

                    input.AdvanceTo(buffer.End);
                    continue;
                }
                
                byte[] raw = buffer.ToArray();
                byte[] encrypted = Encrypt(raw);

                await output.WriteAsync(encrypted);
                input.AdvanceTo(buffer.End);

            }
        }

        private async Task DecryptAsync(PipeReader input, PipeWriter output)
        {
            while (!_disconnectTokenSource.IsCancellationRequested)
            {
                Memory<byte> memory = output.GetMemory(_minimumBufferSize);

                ReadResult result;

                try
                {
                    result = await input.ReadAsync(_disconnectTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to send message: {0}", ex);
                    break;
                }

                if (result.IsCanceled || result.IsCompleted)
                    break;

                ReadOnlySequence<byte> buffer = result.Buffer;

                // Check if encryption is enabled, else just pass through data
                if (!_enabled)
                {
                    foreach (var segment in buffer)
                        await output.WriteAsync(segment);

                    input.AdvanceTo(buffer.End);
                    continue;
                }
                
                byte[] raw = buffer.ToArray();
                byte[] decrypted = Decrypt(raw);

                await output.WriteAsync(decrypted);
                input.AdvanceTo(buffer.End);                
            }
        }

        protected virtual byte[] Encrypt(byte[] raw)
        {
            byte[] encrypted = new byte[raw.Length];

            using (var aes = Aes.Create())
            {
                aes.Padding = PaddingMode.None;
                aes.Key = _key;
                aes.IV = _encryptIV;
                aes.Mode = CipherMode.ECB;

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte temp;

                    for (int offset = 0; offset < raw.Length; offset++)
                    {
                        // Encrypt IV
                        byte[] enc = encryptor.TransformFinalBlock(_encryptIV, 0, 16);

                        // XOR with raw data
                        temp = (byte)(enc[0] ^ raw[offset]);

                        // Shift IV and add new bits
                        Array.Copy(_encryptIV, 1, _encryptIV, 0, 15);
                        _encryptIV[15] = temp;

                        encrypted[offset] = temp;
                    }
                }
            }

            return encrypted;
        }
        
        protected virtual byte[] Decrypt(byte[] raw)
        {
            byte[] decrypted = new byte[raw.Length];

            using (var aes = Aes.Create())
            {
                aes.Padding = PaddingMode.None;
                aes.Key = _key;
                aes.IV = _decryptIV;
                aes.Mode = CipherMode.ECB;

                using (var decryptor = aes.CreateEncryptor())
                {
                    byte temp;

                    for (int offset = 0; offset < raw.Length; offset++)
                    {
                        // Encrypt IV
                        byte[] enc = decryptor.TransformFinalBlock(_decryptIV, 0, 16);

                        // XOR with raw data
                        temp = (byte)(enc[0] ^ raw[offset]);

                        // Shift IV and add new bits
                        Array.Copy(_decryptIV, 1, _decryptIV, 0, 15);

                        _decryptIV[15] = raw[offset];

                        decrypted[offset] = temp;
                    }

                }
            }

            return decrypted;
        }

        private void Test()
        {
            byte[] enc = null;
            byte[] dec = null;
            using (var aes = Aes.Create())
            {
                aes.Padding = PaddingMode.None;
                aes.Key = _key;
                aes.IV = _encryptIV;
                
                using (var encryptor = aes.CreateEncryptor())
                {
                    enc = encryptor.TransformFinalBlock(_encryptIV, 0, 16);
                }

            }

            using (var aes = Aes.Create())
            {
                aes.Padding = PaddingMode.None;
                aes.Key = _key;
                aes.IV = _encryptIV;

                using (var decryptor = aes.CreateEncryptor())
                {
                    dec = decryptor.TransformFinalBlock(_encryptIV, 0, 16);
                }

            }
        }

        public void Close()
        {
            _disconnectTokenSource?.Cancel();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
