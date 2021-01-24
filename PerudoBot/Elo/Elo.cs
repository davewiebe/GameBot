using System;
using System.Collections.Generic;
using System.Text;

namespace PerudoBot.Elo
{
    public class EloPlayer
    {
        public string name;

        public int Place = 0;
        public int EloPre = 0;
        public int EloPost = 0;
        public int EloChange = 0;
    }

    public class EloMatch
    {
        private List<EloPlayer> players = new List<EloPlayer>();

        public void AddPlayer(string name, int place, int Elo)
        {
            EloPlayer player = new EloPlayer
            {
                name = name,
                Place = place,
                EloPre = Elo
            };

            players.Add(player);
        }

        public int GetElo(string name)
        {
            foreach (EloPlayer p in players)
            {
                if (p.name == name)
                    return p.EloPost;
            }
            return 1500;
        }

        public int GetEloChange(string name)
        {
            foreach (EloPlayer p in players)
            {
                if (p.name == name)
                    return p.EloChange;
            }
            return 0;
        }

        public void CalculateElos()
        {
            int n = players.Count;
            float K = 20 / (float)(n - 1);

            for (int i = 0; i < n; i++)
            {
                int curPlace = players[i].Place;
                int curElo = players[i].EloPre;

                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        int opponentPlace = players[j].Place;
                        int opponentElo = players[j].EloPre;

                        //work out S
                        float S;
                        if (curPlace < opponentPlace)
                            S = 1.0F;
                        else if (curPlace == opponentPlace)
                            S = 0.5F;
                        else
                            S = 0.0F;

                        //work out EA
                        float EA = 1 / (1.0f + (float)Math.Pow(10.0f, (opponentElo - curElo) / 400.0f));

                        //calculate Elo change vs this one opponent, add it to our change bucket
                        //I currently round at this point, this keeps rounding changes symetrical between EA and EB, but changes K more than it should
                        players[i].EloChange += (int)Math.Round(K * (S - EA));
                    }
                }
                //add accumulated change to initial Elo for final Elo
                players[i].EloPost = players[i].EloPre + players[i].EloChange;
            }
        }
    }
}