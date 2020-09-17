namespace PerudoBot.Data
{
    public class Note
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public virtual Game Game { get; set; }
        public string Text { get; set; }
        public string Username { get; internal set; }
    }
}
