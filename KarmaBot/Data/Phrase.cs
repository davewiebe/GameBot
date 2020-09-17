namespace PerudoBot.Data
{
    public class Phrase
    {
        public int Id { get; set; }
        public virtual KeyPhrase KeyPhrase { get; set; }
        public int? KeyPhraseId { get; set; }
        public string Text { get; set; }
    }
}
