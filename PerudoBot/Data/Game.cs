﻿using PerudoBot.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PerudoBotTests.PerudoGameServiceTests")]
namespace PerudoBot.Data
{
    public class Game
    {
        public int Id { get; set; }
        public int State { get; set; }

        public int? PlayerTurnId { get; set; }
        public int NumberOfDice { get; internal set; }
        public int Penalty { get; internal set; }
        public bool RandomizeBetweenRounds { get; internal set; }
        public bool WildsEnabled { get; internal set; }
        public ulong ChannelId { get; set; }
        public int RoundStartPlayerId { get; internal set; }
        public int ExactCallBonus { get; internal set; }
        public bool CanCallExactAnytime { get; internal set; }
        public bool CanCallLiarAnytime { get; internal set; }
        public int ExactCallPenalty { get; internal set; }
        public bool CanBidAnytime { get; internal set; }
        public bool Palifico { get; internal set; }
        public bool NextRoundIsPalifico { get; internal set; }
        public bool IsRanked { get; internal set; }
        public DateTime DateCreated { get; internal set; }
        public ulong GuildId { get; internal set; }
        public string Winner { get; internal set; }
        public bool FaceoffEnabled { get; internal set; }

        public virtual List<Note> Notes { get; set; }

        public DateTime DateStarted { get; internal set; }

        public DateTime DateFinished { get; internal set; }
        public bool CanCallExactToJoinAgain { get; internal set; }
        public ulong StatusMessage { get; internal set; }

        public virtual ICollection<GamePlayer> GamePlayers { get; set; }

        public virtual ICollection<Round> Rounds { get; set; }

        public int LowestPip { get; internal set; }
        public int HighestPip { get; internal set; }

        public double? DurationInSeconds { get; set; }

        [NotMapped]
        public Round CurrentRound => Rounds.LastOrDefault();

        public int CurrentRoundNumber => CurrentRound?.RoundNumber ?? 0;

        public bool PenaltyGainDice { get; internal set; }

        public bool TerminatorMode { get; internal set; }
        public int DealCurrentGamePlayerId { get; internal set; }

        public void StartGame()
        {
            State = (int)GameState.InProgress;
            DateStarted = DateTime.Now;

            var startplayerId = GamePlayers
                .OrderBy(gp => gp.TurnOrder)
                .First()
                .Id;

            PlayerTurnId = startplayerId;
            RoundStartPlayerId = startplayerId;
        }

        public void EndGame()
        {
            State = (int)GameState.Finished;
            DateFinished = DateTime.Now;
            DurationInSeconds = (DateFinished - DateStarted).TotalSeconds;
            Winner = GamePlayers.Single(gp => gp.NumberOfDice > 0).Player.Username;
        }
    }
}