namespace PerudoBot.Extensions
{
    public static class DiceExtensions
    {
        public static string GetEmoji(this int? die)
        {
            if (die == null) return "";
            if (die == 1) return ":one:";
            if (die == 2) return ":two:";
            if (die == 3) return ":three:";
            if (die == 4) return ":four:";
            if (die == 5) return ":five:";
            if (die == 6) return ":six:";
            return die.ToString();
        }

        public static string GetEmoji(this int die)
        {
            return GetEmoji((int?)die);
        }
    }
}