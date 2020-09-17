namespace PerudoBot.Data
{
    public class Bid
    {
        public int Id { get; set; }
        public virtual Player Player { get; set; }
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int Quantity { get; set; }
        public int Pips { get; set; }
        public string Call { get; set; }
    }
}
