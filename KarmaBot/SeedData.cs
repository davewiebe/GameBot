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
using PerudoBot.Data;
using System.Linq;
using System.Collections.Generic;

namespace PerudoBot
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
                    Text = Enums.KeyPhrases.SlowDown , 
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "Slow down, buddy." }
                    }
                },
                new KeyPhrase{
                    Text = Enums.KeyPhrases.ThankYouFromBot,
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "Awe thanks." },
                        new Phrase{ Text = ":)" },
                    }
                },
                new KeyPhrase{
                    Text = Enums.KeyPhrases.GetCurrentKarma,
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "<thing> has <totalkarma> karma" }
                    }
                },
                new KeyPhrase{
                    Text = Enums.KeyPhrases.KarmaIncreased,
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "<thing>'s karma has increased to <totalkarma>" }
                    }
                },
                new KeyPhrase{
                    Text = Enums.KeyPhrases.KarmaDecreased,
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "<thing>'s karma has decreased to <totalkarma>" }
                    }
                },
                new KeyPhrase{
                    Text = Enums.KeyPhrases.HoldUp,
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "Hold up... I see what you did there." }
                    }
                },
                new KeyPhrase{
                    Text = Enums.KeyPhrases.BotIsHurt,
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "I thought we were friends." }
                    }
                },
                new KeyPhrase{
                    Text = Enums.KeyPhrases.RightBackAtYou,
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "Right back at you!" }
                    }
                },
                new KeyPhrase{
                    Text = Enums.KeyPhrases.HeckleCoffeeBot,
                    Phrases = new List<Phrase>{
                        new Phrase{ Text = "Hey <coffeebot>, I hope Andrey's paying you!" }
                    }
                },
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
