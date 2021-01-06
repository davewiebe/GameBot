using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PerudoBot.Data
{
    public class Player
    {
        public int Id { get; set; }

        [DefaultValue(0)]
        public ulong UserId { get; set; }

        [DefaultValue(0)]
        public ulong GuildId { get; set; }

        public string Username { get; set; }

        public string Nickname { get; set; }

        public bool IsBot { get; internal set; }

        public int? EloRatingStandard { get; set; }

        public int? EloRatingSuddenDeath { get; set; }

        public int? EloRatingVariable { get; set; }

        public ICollection<GamePlayer> GamesPlayed { get; set; }
    }
}