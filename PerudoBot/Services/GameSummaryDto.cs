using PerudoBot.Modules;
using System;
using System.Collections.Generic;

namespace PerudoBot.Services
{
    public class GameSummaryDto
    {
        public int GameId { get; set; }
        public GameState GameState { get; set; }
        public bool IsRanked { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateFinished { get; set; }
        public double DurationInSeconds { get; set; }
        public double DurationInMinutes => Math.Round(DurationInSeconds / 60);
        public int RoundCount { get; set; }
        public int PlayerCount { get; set; }
        public int Penalty { get; set; }
        public List<GamePlayerSummaryDto> GamePlayers { get; set; }
        public List<NoteDto> Notes { get; set; }
    }
}