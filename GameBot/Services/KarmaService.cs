using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameBot.Services
{
    public class KarmaService
    {
        private SocketCommandContext _context;
        private GameBotDbContext _db;
        private UserService _userService;

        public KarmaService(SocketCommandContext context, GameBotDbContext db)
        {
            _context = context;
            _db = db;
            _userService = new UserService(_context);
        }

        public bool HasGivenKarmaRecently(string thing, int minutes)
        {
            var timeDifference = TimeInMinutesSinceLastKarmaSent(thing);

            if (timeDifference < minutes)
            {
                return true;
            }
            return false;
        }

        public string GiveKarma(string thing, int karmaPoints)
        {

            SaveKarma(thing, karmaPoints, _context.Message.Author.Id);

            var totalPoints = GetTotalKarmaPoints(thing);

            return $"{_userService.GetNicknameIfUser(thing)}'s karma has {(karmaPoints > 0 ? "increased" : "decreased")} to {totalPoints}";

        }

        public int GetTotalKarmaPoints(string thing)
        {
            return _db.Karma.AsQueryable().Where(x => x.Thing == thing).Select(x => x.Points).Sum();
        }

        private double TimeInMinutesSinceLastKarmaSent(string thing)
        {
            var fromUserId = _context.Message.Author.Id;
            var mostRecentKarma = _db.Karma.AsQueryable()
                .Where(x => x.FromUserId == fromUserId)
                .Where(x => x.Thing == thing)
                .ToList()
                .LastOrDefault();

            if (mostRecentKarma == null) return double.MaxValue;

            var timeDifference = DateTime.Now - mostRecentKarma.GivenOn;
            return timeDifference.TotalMinutes;
        }

        private void SaveKarma(string thing, int karmaPoints, ulong from)
        {
            var karma = new Karma
            {
                Points = karmaPoints,
                Thing = thing,
                FromUserId = from,
                GivenOn = DateTime.Now
            };
            _db.Karma.Add(karma);
            _db.SaveChanges();
        }
    }
}
