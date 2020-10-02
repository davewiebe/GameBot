using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PerudoBot.Data
{
    public abstract class Action
    {
        public int Id { get; set; }
        public virtual Player Player { get; set; }
        public int PlayerId { get; set; }

        public int RoundId { get; set; }
        public Round Round { get; set; }

        //public int GameId { get; set; }
        //public virtual Game Game { get; set; }
        public int? ParentActionId { get; set; }

        [ForeignKey("ParentActionId")]
        public virtual Action ParentAction { get; set; }

        public bool IsSuccess { get; set; }
        public bool IsOutOfTurn { get; set; }
    }

    public class Bid : Action
    {
        public int Quantity { get; set; }
        public int Pips { get; set; }
    }

    public class LiarCall : Action
    {
    }

    public class ExactCall : Action
    {
    }
}