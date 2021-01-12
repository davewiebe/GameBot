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
            var number = (playerId + id) % 25;

            switch (number)
            {
                case 1: return ":snowman:";
                case 2: return ":snowman:";
                case 3: return ":snowboarder:";
                case 4: return ":skier:";
                case 5: return ":mountain_snow:";
                case 6: return ":snowman:";
                case 7: return ":skier:";
                case 8: return ":snowboarder:";
                case 9: return ":snowman:";
                case 10: return ":snowman2:";
                case 11: return ":snowman:";
                case 12: return ":snowman:";
                case 13: return ":cold_face:";
                case 14: return ":snowman:";
                case 15: return ":snowboarder:";
                case 16: return ":skier:";
                case 17: return ":snowman:";
                case 18: return ":snowman:";
                case 19: return ":skier:";
                case 20: return ":snowboarder:";
                case 21: return ":snowflake:";
                case 22: return ":snowman:";
                case 23: return ":scarf:";
                case 24: return ":snowman:";
                default:
                    return ":snowman:";
            }
        }

    }
}