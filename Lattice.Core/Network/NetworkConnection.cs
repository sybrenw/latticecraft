using Lattice.Core.Network.Pipelines;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lattice.Core.Network
{
    public abstract class NetworkConnection : IConnection, IDisposable
    {
        private static ILogger Logger { get; } = AppLogging.CreateLogger<NetworkConnection>();
                
        /* Private members */
        private Socket _socket;
        private IDxPipe _pipe;
        private CancellationTokenSource _disconnectTokenSource;
        static SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        // Socket is connected
        public bool Connected { get; private set; } = true;

        public bool KeepAlive { get; set; } = false;

        public string IpAddress { get; }

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;

        public NetworkConnection()
        {

        }

        public NetworkConnection(Socket socket, CancellationToken disconnectToken = default)
        {
            _socket = socket;
            _disconnectTokenSource = CancellationTokenSource.CreateLinkedTokenSource(disconnectToken);

            _pipe = new DxSocketPipe(_socket, _disconnectTokenSource.Token);

            // Get IP Address
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            if (endPoint != null)
                IpAddress = endPoint.Address.ToString();
        }

        public NetworkConnection(Socket socket, IDxPipe pipe, CancellationToken disconnectToken = default)
        {
            _socket = socket;
            _pipe = pipe;
            _disconnectTokenSource = CancellationTokenSource.CreateLinkedTokenSource(disconnectToken);
            
            var socketPipe = new DxSocketPipe(_socket, _disconnectTokenSource.Token);
            _pipe = new DxPipeLine(socketPipe, pipe);

            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            if (endPoint != null)
                IpAddress = endPoint.Address.ToString();
        }

        public async Task ConnectAsync(string ip, int port)
        {
            do
            {
                _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                while (!Connected && !_disconnectTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        await _socket.ConnectAsync(ip, port);
                        Logger.LogInformation("Connected to: {0}", ip);
                        _pipe = new DxSocketPipe(_socket, _disconnectTokenSource.Token);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Failed to connect: {0}", ex);
                        await Task.Delay(5000);
                    }
                }
                await RunAsync();
            } while (!_disconnectTokenSource.IsCancellationRequested && KeepAlive);
        }

        public virtual async Task RunAsync()
        {
            // Run pipe
            Task process = _pipe.RunAsync();
            // Initialize connection
            Task init = Initialize();
            // Reading from pipe
            Task reading = ReadAsync();
            
            // Await all
            await Task.WhenAll(process, reading, init);
        }

        private async Task ReadAsync()
        {
            try
            {
                while (!_disconnectTokenSource.IsCancellationRequested)
                {
                    ReadResult result = await _pipe.Output.ReadAsync(_disconnectTokenSource.Token);
                    
                    if (result.IsCanceled || result.IsCompleted)
                        break;
                    
                    ReadOnlySequence<byte> buffer = result.Buffer;
                    SequencePosition consumed = buffer.Start;

                    buffer = ReadSequence(buffer);

                    _pipe.Output.AdvanceTo(buffer.Start, buffer.End);                    
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                Logger.LogError("Reading failed: {0}", e);
            }

            _pipe.Output.Complete();
        }
        
        private ReadOnlySequence<byte> ReadSequence(ReadOnlySequence<byte> buffer)
        {
            SequenceReader<byte> reader = new SequenceReader<byte>(buffer);
            
            while (!reader.End)
            {
                var remaining = reader.Remaining;
                
                var packet = ReadPacket(ref reader);

                if (remaining == reader.Remaining)
                    break;

                if (packet  == null)
                    continue;

                Logger.LogInformation("Received packet {0} ({1} bytes)", packet.GetType().Name, packet.Length);

                // Read and process packet
                packet.Client = this;
                long start = reader.Consumed;
                packet.Read(ref reader);
                long end = reader.Consumed;

                if (end - start < packet.Length)
                    reader.Advance(packet.Length - (end - start));

                Task.WaitAll(packet.ProcessAsync());
            }

            buffer = buffer.Slice(reader.Position);

            return buffer;
        }

        public async Task SendPacketAsync(SendablePacket packet)
        {
            await _writeLock.WaitAsync();
            using (var ms = new MemoryStream())
            {
                try
                {
                    packet.WritePacket(ms);
                    var data = new ReadOnlyMemory<byte>(ms.GetBuffer(), 0, (int)ms.Position);

                    await _pipe.Input.WriteAsync(packet.Header);
                    await _pipe.Input.WriteAsync(data);
                    await _pipe.Input.FlushAsync();

                    Logger.LogInformation("Sent packet 0x{0:X2} - {1}", packet.PacketId, packet.GetType().Name);
                }
                finally
                {
                    _writeLock.Release();
                }
            }
        }

        public void Disconnect()
        {
            _disconnectTokenSource.Cancel();
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }

        protected virtual byte[] Encrypt(byte[] raw)
        {
            return raw;
        }

        protected virtual byte[] Decrypt(byte[] raw)
        {
            return raw;
        }

        /* Abstract functions */
        protected abstract ReceivablePacket ReadPacket(ref SequenceReader<byte> reader);
        protected abstract Task Initialize();
    }
}
