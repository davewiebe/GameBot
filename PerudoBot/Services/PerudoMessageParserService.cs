using GameBot.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameBot.Services
{
    public class PerudoMessageParserService
    {
        private GameBotDbContext _db;

        public PerudoMessageParserService(GameBotDbContext db)
        {
            _db = db;
        }

        //internal Bid GetBid(string[] bidText)
        //{
        //    int quantity = 0;
        //    int pips = 0;
        //    try
        //    {
        //        quantity = int.Parse(bidText[0]);
        //        pips = int.Parse(bidText[1].Trim('s'));
        //    }
        //    catch
        //    {
        //        return null;
        //    }

        //    if (quantity <= 0) return null;
        //    if (pips < 1 || pips > 6) return null;


        //    var bid = new Bid
        //    {
        //        Call = "",
        //        Pips = pips,
        //        Quantity = quantity,
        //        PlayerId = biddingPlayer.Id,
        //        GameId = game.Id
        //    };
        //    return bid
        //}
    }
}
