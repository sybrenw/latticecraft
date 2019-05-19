using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lattice.Core.Network
{
    public abstract class NetworkListener: IDisposable
    {
        private static ILogger Logger { get; } = AppLogging.CreateLogger<NetworkListener>();

        private Socket _socket;

        /* Currently connected clients */
        private ConcurrentBag<ConnectionState> _connections = new ConcurrentBag<ConnectionState>();
        
        private CancellationTokenSource _disconnectTokenSource;

        public NetworkListener()
        {
            _disconnectTokenSource = new CancellationTokenSource();
        }
        
        public async Task ListenAsync(string ip, int port)
        {
            IPAddress ipAddress;
            if (ip == "*")
                ipAddress = IPAddress.Any;
            else
                ipAddress = IPAddress.Parse(ip);

            await ListenAsync(ipAddress, port);
        }

        public async Task ListenAsync(IPAddress ip, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(ip, port);
            _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(endPoint);
            _socket.Listen(1000);
            
            while (!_disconnectTokenSource.IsCancellationRequested)
            {
                try
                {
                    // Wait for client connection
                    Socket socket = await _socket.AcceptAsync();

                    // Create new client and start processing
                    var client = CreateConnection(socket, _disconnectTokenSource.Token);
                    var state = new ConnectionState(client);
                    state.Task = Task.Run(client.RunAsync);

                    // Add client to collection
                    _connections.Add(state);

                    Logger.LogInformation("New connection: " + socket?.RemoteEndPoint?.ToString());
                }
                catch (SocketException ex)
                {
                    Logger.LogError("Failed to accept socket: {0}", ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to create new connection: {0}", ex);
                }
            }

            // Cleanup
            _socket.Close();
            _socket.Dispose();
            _socket = null;

            Logger.LogInformation("Waiting for clients to disconnect");            
            Logger.LogInformation("Network stopped");
        }

        public void Close()
        {
            _disconnectTokenSource.Cancel();
        }

        public void Dispose()
        {
            Close();
            _socket?.Dispose();
        }

        /* Abstract methods */
        protected abstract IConnection CreateConnection(Socket socket, CancellationToken disconnectToken);
    }
}
