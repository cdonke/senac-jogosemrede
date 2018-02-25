using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace WebSocketsServer.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
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
                    case WebSocketMessageType.Text:

                        userMessage = string.Format("Você enviou: " + userMessage + " às " + DateTime.Now.ToLongTimeString());

                        await SendAsync(webSocket, userMessage);
                        break;
                }

            });
        }

        private async Task OnConnected(WebSocket socket)
        {
            await SendAsync(socket, "Connected succesfully.");
        }
        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, string> handleMessage)
        {
            while (socket.State == WebSocketState.Open)
            {
                var token = CancellationToken.None;
                var buffer = new ArraySegment<Byte>(new Byte[4096]);
                var received = await socket.ReceiveAsync(buffer, token);

                string userMessage = Encoding.UTF8.GetString(buffer.Array, 0, received.Count);

                handleMessage(received, userMessage);
            }
        }
        private async Task SendAsync(WebSocket socket, string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class WebSocketMiddlewareExtensions
    {
        public static IApplicationBuilder UseSENACWebSockets<T>(this IApplicationBuilder app, string path)
        {
            return UseSENACWebSockets<T>(app, path, new WebSocketOptions());
        }
        public static IApplicationBuilder UseSENACWebSockets<T>(this IApplicationBuilder app, string path, WebSocketOptions options)
        {
            var pipeline = app.UseWebSockets(options);
            return app.Map(new PathString(path), _ => _.UseMiddleware<T>());
        }
    }
}
