namespace PerudoBot.Services
{
    public class GamePlayerSummaryDto
    {
        public string Nickname { get; set; }
        public int? Rank { get; set; }
        public int? PostGameEloRating { get; set; }
        public int? EloChange { get; set; }
    }
}