using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SenacGames.Middlewares
{
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