namespace PerudoBot.Data
{
    public class Rattle
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Deathrattle { get; set; }
        public string Winrattle { get; internal set; }
        public string Tauntrattle { get; internal set; }
    }
}
