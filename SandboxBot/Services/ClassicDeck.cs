using System;
using System.Collections.Generic;
using System.Text;

namespace PerudoBot.Services
{
    internal class ClassicCard
    {
        public int Value;
        public static string[] SuitsArray = new string[] { "Hearts", "Diamonds", "Clubs", "Spades" };
        public string Suit;

        public ClassicCard(int value, string suit)
        {
            Value = value;
            Suit = suit;
        }

        public ClassicCard(string input)
        {
            string tempValue = "";
            string suitSentence = "";
            switch (Value)
            {
                case 11:
                    tempValue = "Jack";
                    break;
                case 12:
                    tempValue = "Queen";
                    break;
                case 13:
                    tempValue = "King";
                    break;
                case 14:
                    tempValue = "Ace";
                    break;
                default:
                    tempValue = Value.ToString();
                    break;
            }
            switch (Suit)
            {
                case "Hearts":
                    suitSentence = " of Hearts";
                    break;
                case "Diamonds":
                    suitSentence = " of Diamonds";
                    break;
                case "Clubs":
                    suitSentence = " of Clubs";
                    break;
                case "Spades":
                    suitSentence = " of Spades";
                    break;
            }
            input = tempValue + suitSentence;
        }
    }

    public class ClassicDeck
    {
        private ClassicCard[] deck = new ClassicCard[52];

        public ClassicDeck()
        {
            CreateDeck();
        }
        private void CreateDeck()
        {
            int index = 0;
            foreach (string suit in ClassicCard.SuitsArray)
            {
                for (int value = 2; value <= 14; value++)
                {
                    ClassicCard card = new ClassicCard(value, suit);
                    deck[index] = card;
                    index++;
                }
            }
        }

        public void PrintDeck()
        {
            for (int i = 0; i < 52; i++)
            {
                Console.WriteLine($"{deck[i].Value} {deck[i].Suit}");
            }
        }
    }
}
