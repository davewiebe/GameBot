using System;
using System.IO;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GameBot.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace GameBot
{
    partial class Program
    {
        private static void SeedDatabase()
        {
            var db = new GameBotDbContext();
            db.Database.EnsureCreated();

            var keyPhrases = new List<KeyPhrase>
            {
                new KeyPhrase{ 
                    Text = "Slow down" , 
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "Slow down, buddy." }
                    }
                },
                new KeyPhrase{
                    Text = "Thankyou from bot" ,
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "Awe thanks." },
                        new Phrase{ Text = ":)" },
                    }
                }
            };

            foreach (var keyPhrase in keyPhrases)
            {
                if (db.KeyPhrase.SingleOrDefault(x => x.Text == keyPhrase.Text) == null)
                {
                    db.KeyPhrase.Add(keyPhrase);
                }
            }

            db.SaveChanges();
        }
    }
}
