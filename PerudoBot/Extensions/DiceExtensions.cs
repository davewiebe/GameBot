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
            var number = (playerId + id) % 28;

            switch (number)
            {
                case 1: return ":rabbit2:";
                case 2: return ":sunflower:";
                case 3: return ":butterfly:";
                case 4: return ":hedgehog:";
                case 5: return ":rainbow:";
                case 6: return ":lady_beetle:";
                case 7: return ":sun_with_face:";
                case 8: return ":rose:";
                case 9: return ":duck:";
                case 10: return ":bee:";
                case 11: return ":tulip:";
                case 12: return ":sheep:";
                case 13: return ":swan:";
                case 14: return ":hatched_chick:";
                case 15: return ":rabbit:";
                case 16: return ":herb:";
                case 17: return ":ear_of_rice:";
                case 18: return ":blossom:";
                case 19: return ":cherry_blossom:";
                case 20: return ":snail:";
                case 21: return ":four_leaf_clover:";
                case 22: return ":leaves:";
                case 23: return ":seedling:";
                case 24: return ":beetle:";
                case 25: return ":bug:";
                case 26: return ":cricket:";
                case 27: return ":spider:";
                default:
                    return ":rabbit2:";
            }
        }

    }
}