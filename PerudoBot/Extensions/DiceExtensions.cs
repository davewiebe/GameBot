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
            if (die == 7) return ":seven:";
            if (die == 8) return ":eight:";
            if (die == 9) return ":nine:";
            return die.ToString();
        }

        public static string GetEmoji(this int die)
        {
            return GetEmoji((int?)die);
        }

        public static string GetChristmasEmoji(this int playerId, int id)
        {
            var number = (playerId + id) % 13;

            switch (number)
            {
                case 0: return ":snowman:";
                case 1: return ":snowman:";
                case 2: return ":snowman:";
                case 3: return ":snowman:";
                case 4: return ":snowman:";
                case 5: return ":snowman:";
                case 6: return ":snowman2:";
                case 7: return ":deer:";
                case 8: return ":mx_claus:";
                case 9: return ":mx_claus:";
                case 10: return ":santa:";
                case 11: return ":elf:";
                case 12: return ":christmas_tree:";
                default:
                    return ":snowman:";
            }
        }

    }
}