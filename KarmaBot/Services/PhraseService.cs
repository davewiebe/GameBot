using PerudoBot.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.Services
{
    public class PhraseService
    {
        private GameBotDbContext _db;
        private List<(string, string)> _replacements;

        public PhraseService(GameBotDbContext db)
        {
            _db = db;
            _replacements = new List<(string, string)>();
        }

        public void AddReplacement(string replace, string value)
        {
            _replacements.RemoveAll(x => x.Item1 == replace);
            _replacements.Add((replace, value));
        }

        public string GetPhrase(string keyPhrase)
        {
            var r = new Random();

            var phrases = _db.KeyPhrase
                .Include(x => x.Phrases)
                .Single(x => x.Text == keyPhrase)
                .Phrases;

            var text = phrases.ElementAt(r.Next(0, phrases.Count())).Text;

            foreach (var replacement in _replacements)
            {
                text = text.Replace(replacement.Item1, replacement.Item2);
            }
            return text;
        }
    }
}
