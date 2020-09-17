using System;
using System.Collections.Generic;
using System.Text;

namespace PerudoBot.Data
{
    public class Karma
    {
        public int Id { get; set; }
        public ulong Server { get; set; }
        public string Thing { get; set; }
        public int Points { get; set; }
        public ulong FromUserId { get; set; }
        public DateTime GivenOn { get; set; }
    }
}
