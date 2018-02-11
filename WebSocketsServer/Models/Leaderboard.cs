using System;
using Newtonsoft.Json;

namespace WebSocketsServer.Models
{
    public class Leaderboard
    {
        public Guid? Id { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "score")]
        public int Score { get; set; }

        public string game { get; set; }

        public override string ToString()
        {
            return $"{Id}, {Username}, {Score}";
        }
    }
}
