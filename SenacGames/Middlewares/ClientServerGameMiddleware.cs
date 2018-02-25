using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SenacGames.Middlewares
{
    public class ClientServerGameMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ClientServerGameMiddleware> _logger;
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public ClientServerGameMiddleware(RequestDelegate next, ILogger<ClientServerGameMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {

            if (!httpContext.WebSockets.IsWebSocketRequest)
                return;

            var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            await OnConnected(webSocket);

            await Receive(webSocket, async (result, userMessage) =>
            {
                switch (result.MessageType)
                {
                    case WebSocketMessageType.Close:
                        try
                        {
                            var closedSessions =
                                _sockets
                                    .Where(q => q.Value.State == WebSocketState.Closed || q.Value.State == WebSocketState.CloseReceived)
                                    .Select(q => q.Key).ToList();

                            foreach (var session in closedSessions)
                                _sockets.Remove(session, out WebSocket _);

                            foreach (var session in closedSessions)
                            {
                                var msg = new Models.CLientServerGame
                                {
                                    action = "CLOSED",
                                    sender = session
                                };
                                await broadcastMessage(msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.ToString());
                        }
                        break;

                    case WebSocketMessageType.Text:
                        try
                        {
                            var msg = JsonConvert.DeserializeObject<Models.CLientServerGame>(userMessage);
                            var session = _sockets[msg.sender];
                            await broadcastMessage(msg, msg.sender);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.ToString());
                        }
                        break;
                }

            });
        }

        private async Task OnConnected(WebSocket socket)
        {
            // Criar novo jogador
            var o = new Models.CLientServerGame
            {
                sender = CreateConnectionId(),
                action = "ICONNECTED"
            };
            _sockets.TryAdd(o.sender, socket);
            await SendAsync(socket, o);

            // Avisar novo jogador aos outors
            o.action = "CONNECTED";
            await broadcastMessage(o, o.sender);


            // Avisar todos os jogadores ao novo
            var players = _sockets
                .Where(q => q.Key != o.sender)
                .Select(q => new Models.CLientServerGame
                {
                    sender = q.Key,
                    action = "PLAYER"
                });

            var player = new Models.CLientServerGame
            {
                action = "PLAYERS",
                sender = o.sender,
                message = JsonConvert.SerializeObject(players)
            };
            await SendAsync(socket, player);
        }
        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, string> handleMessage)
        {
            while (socket.State == WebSocketState.Open)
            {
                var token = CancellationToken.None;
                var buffer = new ArraySegment<Byte>(new Byte[4096]);
                var received = await socket.ReceiveAsync(buffer, token);

                string userMessage = Encoding.UTF8.GetString(buffer.Array, 0, received.Count);
                _logger.LogInformation(userMessage);
                handleMessage(received, userMessage);
            }
        }
        private async Task SendAsync(WebSocket socket, Models.CLientServerGame obj)
        {
            var message = JsonConvert.SerializeObject(obj);
            _logger.LogInformation(message);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }


        async Task broadcastMessage(Models.CLientServerGame obj, string sessionID)
        {
            foreach (var _session in _sockets.Where(q => q.Key != sessionID))
                await SendAsync(_session.Value, obj);
        }
        async Task broadcastMessage(Models.CLientServerGame obj)
        {
            foreach (var _session in _sockets)
                await SendAsync(_session.Value, obj);
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
