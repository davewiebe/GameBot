using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GameBot.Services
{
    public class PhraseService
    {
        private GameBotDbContext _db;

        public PhraseService(GameBotDbContext db)
        {
            _db = db;
        }

        public string GetPhrase(string keyPhrase)
        {
            var r = new Random();

            var phrases = _db.KeyPhrase
                .Include(x => x.Phrases)
                .Single(x => x.Text == keyPhrase)
                .Phrases;

            return phrases.ElementAt(r.Next(0, phrases.Count())).Text;
        }
    }
}
