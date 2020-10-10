using PerudoBot.Data;

namespace PerudoBot.Services
{
    public class LiarCallResult
    {
        public bool IsSuccess { get; set; }

        public int ActualQuantity { get; set; }
        public int Penalty { get; set; }

        // public Bid PreviousBid { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class LiarCallRequest
    {
        public Game Game { get; set; }
        public Player Caller { get; set; }
        public Bid PreviousBid { get; set; }
    }
}