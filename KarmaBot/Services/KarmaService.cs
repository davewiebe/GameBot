using Discord.Commands;
using GameBot.Data;
using System;
using System.Linq;

namespace GameBot.Services
{
    public class KarmaService
    {
        private SocketCommandContext _context;
        private GameBotDbContext _db;

        public KarmaService(SocketCommandContext context, GameBotDbContext db)
        {
            _context = context;
            _db = db;
        }

        public bool HasGivenTooMuchKarmaRecently(ulong userId, int maxKarma, int minutes)
        {
            return HasGivenTooMuchKarmaRecently(userId.ToString(), maxKarma, minutes);
        }
        public bool HasGivenTooMuchKarmaRecently(string thing, int maxKarma, int minutes)
        {
            var numberOfKarma = GetNumberOfKarmaSentRecently(thing, minutes);

            if (numberOfKarma >= maxKarma)
            {
                return true;
            }
            return false;
        }

        public int GetTotalKarmaPoints(ulong userId)
        {
            return GetTotalKarmaPoints(userId.ToString());
        }

        public int GetTotalKarmaPoints(string thing)
        {
            return _db.Karma.AsQueryable()
                .Where(x => x.Thing == thing)
                .Where(x => x.Server == _context.Guild.Id)
                .Select(x => x.Points).Sum();
        }

        private double GetNumberOfKarmaSentRecently(string thing, int minutes)
        {
            var fiveMinutesAgo = DateTime.Now.AddMinutes(-1 * minutes);
            var fromUserId = _context.Message.Author.Id;
            var mostRecentKarma = _db.Karma.AsQueryable()
                .Where(x => x.FromUserId == fromUserId)
                .Where(x => x.Thing == thing)
                .Where(x => x.Server == _context.Guild.Id)
                .Where(x => x.GivenOn >= fiveMinutesAgo)
                .Sum(x => Math.Abs(x.Points));

            return mostRecentKarma;
        }

        public void SaveKarma(ulong userId, int karmaPoints, ulong from)
        {
            SaveKarma(userId.ToString(), karmaPoints, from);
        }
        public void SaveKarma(string thing, int karmaPoints, ulong from)
        {
            var karma = new Karma
            {
                Server = _context.Guild.Id,
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
