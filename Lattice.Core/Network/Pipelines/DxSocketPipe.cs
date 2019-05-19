using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lattice.Core.Network.Pipelines
{
    public class DxSocketPipe : IDxPipe
    {
        private static ILogger Logger { get; } = AppLogging.CreateLogger<DxSocketPipe>();

        const int MINIMUM_BUFFER_SIZE = 512;

        /* IDuplexPipe interface */
        public PipeWriter Input => _inputPipe.Writer;
        public PipeReader Output => _outputPipe.Reader;

        /* Private members */
        private Socket _socket;
        private Pipe _outputPipe;
        private Pipe _inputPipe;

        private int _minimumBufferSize;

        // Is the pipe open
        private bool _connected = true;

        private CancellationTokenSource _disconnectTokenSource;

        public DxSocketPipe(Socket socket, CancellationToken disconnectToken = default) : this(socket, MINIMUM_BUFFER_SIZE, disconnectToken) { }

        public DxSocketPipe(Socket socket, int minimumBufferSize, CancellationToken disconnectToken = default)
        {
            _disconnectTokenSource = CancellationTokenSource.CreateLinkedTokenSource(disconnectToken);

            _socket = socket;
            _socket.NoDelay = true;
            _minimumBufferSize = minimumBufferSize;

            // Create input and output pipe
            _outputPipe = new Pipe();
            _inputPipe = new Pipe();
        }

        public void ConnectPipe(IDxPipe source)
        {
            throw new NotImplementedException();
        }

        public async Task RunAsync()
        {
            Task receiving = ReceiveAsync(_socket, _outputPipe.Writer);
            Task sending = SendAsync(_socket, _inputPipe.Reader);
            await Task.WhenAll(receiving, sending);
        }

        private async Task ReceiveAsync(Socket socket, PipeWriter writer)
        {
            while (_connected)
            {
                Memory<byte> memory = writer.GetMemory(_minimumBufferSize);

                try
                {
                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, _disconnectTokenSource.Token);

                    // Check if disconnected
                    if (bytesRead == 0)
                        break;

                    // Tell pipe how much was read
                    writer.Advance(bytesRead);
                }
                catch (SocketException ex)
                {
                    Logger.LogWarning("Disconnected");
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to receive message: {0}", ex);
                }

                // Flush data to the pipe
                FlushResult result = await writer.FlushAsync();

                // Socket closed
                if (result.IsCompleted)
                    break;
            }

            Logger.LogInformation("Receiving finished");
            // Cleanup
            writer.Complete();
            _disconnectTokenSource?.Cancel();
        }

        private async Task SendAsync(Socket socket, PipeReader reader)
        {
            while (_connected)
            {
                ReadResult result;
                try
                {
                    result = await reader.ReadAsync(_disconnectTokenSource.Token);
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
                foreach(var segment in buffer)
                    await socket.SendAsync(segment, SocketFlags.None);
                
                reader.AdvanceTo(buffer.End);
            }

            Logger.LogInformation("Sending finished");
            reader.Complete();
        }

        public void Close()
        {
            _disconnectTokenSource?.Cancel();
        }
    }
}
