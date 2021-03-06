﻿using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using YGOProAnalyticsServer;
using YGOProAnalyticsServer.Database;
using YGOProAnalyticsServer.DbModels;
using YGOProAnalyticsServer.Services.Others;
using YGOProAnalyticsServer.Services.Others.Interfaces;
using YGOProAnalyticsServerTests.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;

namespace YGOProAnalyticsServerTests.Others
{
    [TestFixture]
    class BanlistServiceTests
    {
        IBanlistService _banlistService;
        YgoProAnalyticsDatabase _db;
        Mock<IMemoryCache> _cacheMock;
        Mock<IAdminConfig> _configMock;

        [SetUp]
        public void SetUp()
        {
            _db = new YgoProAnalyticsDatabase(SqlInMemoryHelper.SqlLiteOptions<YgoProAnalyticsDatabase>());
            _db.Database.EnsureCreated();
            _cacheMock = new Mock<IMemoryCache>();
            _configMock = new Mock<IAdminConfig>();
            _banlistService = new BanlistService(_db, _cacheMock.Object, _configMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        [Test]
        public void CanDeckBeUsedOnGivenBanlist_DeckCanBeUsed_ReturnsTrue()
        {
            var mainDeck = _generateSimplifiedDeckOfCards(40);
            var extraDeck = _generateSimplifiedDeckOfCards(15);
            var sideDeck = _generateSimplifiedDeckOfCards(15);
            Decklist decklist = _genereateDecklistWithAllRequiredData(mainDeck, extraDeck, sideDeck);
            var banlist = new Banlist("2018.05 TCG", 1);

            _db.Decklists.Add(decklist);
            _db.Banlists.Add(banlist);
            _db.SaveChanges();

            var canBeUsed = _banlistService.CanDeckBeUsedOnGivenBanlist(decklist, banlist);

            Assert.IsTrue(canBeUsed);
        }

        [Test]
        public void CanDeckBeUsedOnGivenBanlist_ThereIsBannedCardInMainDeck_ReturnsFalse()
        {
            var mainDeck = _generateSimplifiedDeckOfCards(40);
            var extraDeck = _generateSimplifiedDeckOfCards(15);
            var sideDeck = _generateSimplifiedDeckOfCards(15);
            Decklist decklist = _genereateDecklistWithAllRequiredData(mainDeck, extraDeck, sideDeck);
            var banlist = new Banlist("2018.05 TCG", 1);
            banlist.ForbiddenCards.Add(mainDeck[4]);
            _db.Decklists.Add(decklist);
            _db.Banlists.Add(banlist);
            _db.SaveChanges();

            var canBeUsed = _banlistService.CanDeckBeUsedOnGivenBanlist(decklist, banlist);

            Assert.IsFalse(canBeUsed);
        }

        [Test]
        public void CanDeckBeUsedOnGivenBanlist_ThereIsBannedCardInExtraDeck_ReturnsFalse()
        {
            var mainDeck = _generateSimplifiedDeckOfCards(40);
            var extraDeck = _generateSimplifiedDeckOfCards(15);
            var sideDeck = _generateSimplifiedDeckOfCards(15);
            Decklist decklist = _genereateDecklistWithAllRequiredData(mainDeck, extraDeck, sideDeck);
            var banlist = new Banlist("2018.05 TCG", 1);
            banlist.ForbiddenCards.Add(extraDeck[4]);
            _db.Decklists.Add(decklist);
            _db.Banlists.Add(banlist);
            _db.SaveChanges();

            var canBeUsed = _banlistService.CanDeckBeUsedOnGivenBanlist(decklist, banlist);

            Assert.IsFalse(canBeUsed);
        }

        [Test]
        public void CanDeckBeUsedOnGivenBanlist_ThereIsBannedCardInSideDeck_ReturnsFalse()
        {
            var mainDeck = _generateSimplifiedDeckOfCards(40);
            var extraDeck = _generateSimplifiedDeckOfCards(15);
            var sideDeck = _generateSimplifiedDeckOfCards(15);
            Decklist decklist = _genereateDecklistWithAllRequiredData(mainDeck, extraDeck, sideDeck);
            var banlist = new Banlist("2018.05 TCG", 1);
            banlist.ForbiddenCards.Add(sideDeck[4]);
            _db.Decklists.Add(decklist);
            _db.Banlists.Add(banlist);
            _db.SaveChanges();

            var canBeUsed = _banlistService.CanDeckBeUsedOnGivenBanlist(decklist, banlist);

            Assert.IsFalse(canBeUsed);
        }

        [Test]
        public void CanDeckBeUsedOnGivenBanlist_ThereIsMoreThanOneLimitedCardInDecklist_ReturnsFalse()
        {
            var mainDeck = _generateSimplifiedDeckOfCards(40);
            var extraDeck = _generateSimplifiedDeckOfCards(15);
            var sideDeck = _generateSimplifiedDeckOfCards(14);
            Decklist decklist = _genereateDecklistWithAllRequiredData(mainDeck, extraDeck, sideDeck);
            var banlist = new Banlist("2018.05 TCG", 1);
            banlist.LimitedCards.Add(mainDeck[4]);
            _db.Decklists.Add(decklist);
            _db.Banlists.Add(banlist);
            _db.SaveChanges();
            decklist.SideDeck.Add(_db.Cards.Find(mainDeck[4].Id));
            _db.SaveChanges();

            var canBeUsed = _banlistService.CanDeckBeUsedOnGivenBanlist(decklist, banlist);

            Assert.IsFalse(canBeUsed);
        }

        [Test]
        public void CanDeckBeUsedOnGivenBanlist_ThereIsMoreThanTwoSemiLimitedCardInDecklist_ReturnsFalse()
        {
            var mainDeck = _generateSimplifiedDeckOfCards(40);
            var extraDeck = _generateSimplifiedDeckOfCards(15);
            var sideDeck = _generateSimplifiedDeckOfCards(14);
            Decklist decklist = _genereateDecklistWithAllRequiredData(mainDeck, extraDeck, sideDeck);
            var banlist = new Banlist("2018.05 TCG", 1);
            banlist.LimitedCards.Add(sideDeck[4]);
            _db.Decklists.Add(decklist);
            _db.Banlists.Add(banlist);
            _db.SaveChanges();
            decklist.MainDeck.Add(_db.Cards.Find(sideDeck[4].Id));
            decklist.SideDeck.Add(_db.Cards.Find(sideDeck[4].Id));
            _db.SaveChanges();

            var canBeUsed = _banlistService.CanDeckBeUsedOnGivenBanlist(decklist, banlist);

            Assert.IsFalse(canBeUsed);
        }

        [Test]
        public async Task GetListOfBanlistsNamesAndIdsAsNoTrackingFromCache_ThereIsOneBanlistInDb_WeGetOneDto()
        {
            string banlistName = "2019.01 TCG";
            _db.Banlists.Add(new Banlist(banlistName, 1));
            _db.SaveChanges();
            var _banlistService = new BanlistService(_db, _cacheMock.Object, _configMock.Object);

            var dto = (await _banlistService.GetListOfBanlistsNamesAndIdsAsNoTrackingFromCache(shouldIgnoreCache: true))
                .First();

            Assert.IsTrue(dto.Name == banlistName);
        }

        private static Decklist _genereateDecklistWithAllRequiredData(
            List<Card> mainDeck,
            List<Card> extraDeck,
            List<Card> sideDeck)
        {
            var decklist = new Decklist(mainDeck, extraDeck, sideDeck);
            decklist.Name = "TestName";
            decklist.Archetype = new Archetype(Archetype.Default, true);
            decklist.WhenDecklistWasFirstPlayed = DateTime.Parse("1998-12-12");

            return decklist;
        }

        private List<Card> _generateSimplifiedDeckOfCards(int numberOfCards)
        {
            var deckOfCards = new List<Card>(numberOfCards);
            for (int i = 0; i < numberOfCards; i++)
            {
                deckOfCards.Add(
                   _generateSimplifiedCard(i)
                );
            }

            return deckOfCards;
        }

        private Card _generateSimplifiedCard(int passCode)
        {
            return Card.Create(
                         passCode,
                         "CardName",
                         "cardDesc",
                         "Spell Card",
                         "Normal",
                         "",
                         "",
                         new Archetype(Archetype.Default, true)
                    );
        }
    }
}
