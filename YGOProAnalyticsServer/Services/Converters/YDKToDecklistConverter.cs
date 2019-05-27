﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YGOProAnalyticsServer.Database;
using YGOProAnalyticsServer.DbModels;
using YGOProAnalyticsServer.Services.Converters.Interfaces;

namespace YGOProAnalyticsServer.Services.Converters
{
    /// <summary>Service used to convert ydk string to a new deck containing cards.</summary>
    public class YDKToDecklistConverter : IYDKToDecklistConverter
    {
        private readonly List<Card> cards;

        /// <summary>Initializes a new instance of the <see cref="YDKToDecklistConverter"/> class.</summary>
        /// <param name="db">The database.</param>
        public YDKToDecklistConverter(YgoProAnalyticsDatabase db)
        {
            cards = db.Cards.ToList();
        }

        /// <inheritdoc />
        public Decklist Convert(string ydkString)
        {
            List<Card> mainDeck = new List<Card>();
            List<Card> extraDeck = new List<Card>();
            List<Card> sideDeck = new List<Card>();
            ydkString = ydkString.Replace("\r", "");
            string[] lines = ydkString.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            bool isSide = false;
            bool decklistStarted = false;

            foreach (string line in lines)
            {
                if (line.Contains("#main"))
                {
                    decklistStarted = true;
                }

                if (!decklistStarted)
                {
                    continue;
                }

                if (line == "!side")
                {
                    isSide = true;
                }
                else
                {
                    if (Int32.TryParse(line, out int passCode))
                    {
                        if (_isCorrectPassCode(passCode))
                        {
                            var card = cards.FirstOrDefault(x => x.PassCode == passCode);
                            if (card == null)
                            {
                                continue;
                            }
                            if (!isSide)
                            {
                                if (card.Type.ToLower().Contains("fusion")
                                    || card.Type.ToLower().Contains("xyz")
                                    || card.Type.ToLower().Contains("link")
                                    || card.Type.ToLower().Contains("synchro"))
                                {
                                    extraDeck.Add(card);
                                }
                                else
                                {
                                    mainDeck.Add(card);
                                }
                            }
                            else
                            {
                                sideDeck.Add(card);
                            }
                        }
                    }
                }
            }
            return new Decklist(mainDeck.OrderBy(x=>x.PassCode).ToList(), extraDeck.OrderBy(x => x.PassCode).ToList(), sideDeck.OrderBy(x => x.PassCode).ToList());
        }

        /// <summary>
        /// Determines whether the passcode is correct.
        /// </summary>
        /// <param name="passCode">The pass code.</param>
        /// <returns>
        ///   <c>true</c> if passcode is correct, otherwise <c>false</c>.
        /// </returns>
        private static bool _isCorrectPassCode(int passCode)
        {
            return passCode > 100;
        }
    }
}
