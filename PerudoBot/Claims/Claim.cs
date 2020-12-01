namespace PerudoBot.Claims
{
    public class Claim
    {
        public Claim(Operator op, int quantity, int pips, bool includeWilds = false)
        {
            Operator = op;
            Quantity = quantity;
            Pips = pips;
            IncludeWilds = includeWilds;
        }

        public Operator Operator { get; set; }
        public int Quantity { get; set; }
        public int Pips { get; set; }
        public bool IncludeWilds { get; set; }
    }
}
