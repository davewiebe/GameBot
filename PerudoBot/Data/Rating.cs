namespace PerudoBot.Data
{
    public class EloRating
    {
        public int Id { get; set; }

        public Player Player { get; set; }
        public int PlayerId { get; set; }
        public string GameMode { get; set; }
        public int Rating { get; set; }
    }
}