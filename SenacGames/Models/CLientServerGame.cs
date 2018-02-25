using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SenacGames.Models
{
    public class CLientServerGame
    {
        public string sender { get; set; }
        public string action { get; set; }
        public object message { get; set; }
    }
}
