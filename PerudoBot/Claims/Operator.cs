namespace PerudoBot.Claims
{
    public enum Operator
    {
        GreaterThan = 0,
        LessThan,
        Exactly,
        Approximately
    }

    public static class OperatorExtensions
    {
        public static string ToReadableString(this Operator op)
        {
            return op switch
            {
                Operator.GreaterThan => "greater than",
                Operator.LessThan => "less than",
                Operator.Exactly => "exactly",
                Operator.Approximately => "approximately (+/- 1)",
                _ => "",
            };
        }
    }
}