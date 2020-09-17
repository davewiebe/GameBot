using System.Collections.Generic;

namespace GameBot.Data
{
    public class KeyPhrase
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public virtual List<Phrase> Phrases { get; set; }
    }
}
