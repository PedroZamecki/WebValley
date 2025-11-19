using System.Net;
using System.Net.WebSockets;
using StardewModdingAPI;

namespace WebValley.WebSocket
{
    /// <summary>A simple WebSocket echo server for the mod.</summary>
    internal sealed class WebSocketServer
    {
        private HttpListener? _httpListener;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _serverTask;
        private readonly IMonitor _monitor;
        private readonly string _uri;

        public WebSocketServer(IMonitor monitor, string uri = "http://localhost:8080/")
        {
            _monitor = monitor;
            _uri = uri;
        }

        /// <summary>Start the WebSocket server.</summary>
        public void Start()
        {
            if (_httpListener != null)
            {
                _monitor.Log("WebSocket server is already running.", LogLevel.Warn);
                return;
            }

            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(_uri);
                _httpListener.Start();
                
                _cancellationTokenSource = new CancellationTokenSource();
                _serverTask = AcceptWebSocketConnectionsAsync(_cancellationTokenSource.Token);
                
                _monitor.Log($"WebSocket server started at {_uri}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to start WebSocket server: {ex.Message}", LogLevel.Error);
                _httpListener?.Close();
                _httpListener = null;
            }
        }

        /// <summary>Stop the WebSocket server.</summary>
        public async void Stop()
        {
            if (_httpListener == null)
            {
                _monitor.Log("WebSocket server is not running.", LogLevel.Warn);
                return;
            }

            try
            {
                _cancellationTokenSource?.Cancel();
                
                if (_serverTask != null)
                {
                    await _serverTask;
                }

                _httpListener.Stop();
                _httpListener.Close();
                _httpListener = null;

                _monitor.Log("WebSocket server stopped.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error stopping WebSocket server: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>Accept incoming WebSocket connections and handle them.</summary>
        private async Task AcceptWebSocketConnectionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    HttpListenerContext context = await _httpListener!.GetContextAsync().ConfigureAwait(false);

                    if (!context.Request.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                        continue;
                    }

                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
                    System.Net.WebSockets.WebSocket webSocket = webSocketContext.WebSocket;

                    _monitor.Log("Client connected to WebSocket.", LogLevel.Debug);
                    
                    // Handle this connection in a separate task
                    _ = HandleWebSocketAsync(webSocket, cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Error accepting WebSocket connection: {ex.Message}", LogLevel.Error);
                }
            }
        }

        /// <summary>Handle a WebSocket connection and echo messages.</summary>
        private async Task HandleWebSocketAsync(System.Net.WebSockets.WebSocket webSocket, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cancellationToken
                    ).ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Closing",
                            cancellationToken
                        ).ConfigureAwait(false);
                        _monitor.Log("Client disconnected from WebSocket.", LogLevel.Debug);
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // Echo the message back
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(buffer, 0, result.Count),
                            WebSocketMessageType.Text,
                            result.EndOfMessage,
                            cancellationToken
                        ).ConfigureAwait(false);
                        _monitor.Log($"Echoed message: {System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count)}", LogLevel.Debug);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Server is shutting down
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error handling WebSocket: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                webSocket.Dispose();
            }
        }
    }
}

