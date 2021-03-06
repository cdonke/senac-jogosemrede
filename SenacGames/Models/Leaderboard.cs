﻿using System;
using Newtonsoft.Json;

namespace SenacGames.Models
{
    public class Leaderboard
    {
        [JsonProperty(PropertyName = "id")]
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
