﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YGOProAnalyticsServer.DbModels;
using YGOProAnalyticsServer.DbModels.DbJoinModels;

namespace YGOProAnalyticsServer.Database
{
    public class YgoProAnalyticsDatabase : DbContext
    {
        public DbSet<ServerActivityStatistics> ServerActivityStatistics { get; set; }

        //Banlists
        public DbSet<Banlist> Banlists { get; set; }
        public DbSet<BanlistStatistics> BanlistStatistics { get; set; }

        //Archetypes
        public DbSet<Archetype> Archetypes { get; set; }
        public DbSet<ArchetypeStatistics> ArchetypeStatistics { get; set; }

        //Cards
        public DbSet<Card> Cards { get; set; }
        public DbSet<MonsterCard> MonsterCards { get; set; }
        public DbSet<PendulumMonsterCard> PendulumMonsterCards { get; set; }
        public DbSet<LinkMonsterCard> LinkMonsterCards { get; set; }

        //Decklists
        public DbSet<Decklist> Decklists { get; set; }
        public DbSet<DecklistStatistics> DecklistStatistics { get; set; }


        //MetaData
        public DbSet<AnalysisMetadata> AnalysisMetadata { get; set; }

        public static string ConnectionString(string DBUser, string DBPassword)
        {
            return string.Format(@"
            Server=db;
            Database= YgoProAnalytics;
            User={0};
            Password={1};
            ConnectRetryCount=0", DBUser, DBPassword);
        }


        public YgoProAnalyticsDatabase(DbContextOptions<YgoProAnalyticsDatabase> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ForbiddenCardBanlistJoin>()
              .HasKey(t => new { t.CardId, t.BanlistId });

            modelBuilder.Entity<LimitedCardBanlistJoin>()
              .HasKey(t => new { t.CardId, t.BanlistId });

            modelBuilder.Entity<SemiLimitedCardBanlistJoin>()
              .HasKey(t => new { t.CardId, t.BanlistId });

            modelBuilder.Entity<DecklistsBanlistsJoin>()
            .HasKey(t => new { t.DecklistId, t.BanlistId });

            modelBuilder.Entity<CardInMainDeckDecklistJoin>()
             .HasKey(t => new { t.Id });

            modelBuilder.Entity<CardInExtraDeckDecklistJoin>()
            .HasKey(t => new { t.Id });

            modelBuilder.Entity<CardInSideDeckDecklistJoin>()
            .HasKey(t => new { t.Id });

            modelBuilder.Entity<Card>()
                .HasOne(a => a.MonsterCard)
                .WithOne(b => b.Card)
                .HasForeignKey<MonsterCard>(c => c.CardId);

            modelBuilder.Entity<MonsterCard>()
                .HasOne(a => a.LinkMonsterCard)
                .WithOne(b => b.MonsterCard)
                .HasForeignKey<LinkMonsterCard>(c => c.MonsterCardId);

            modelBuilder.Entity<MonsterCard>()
                .HasOne(a => a.PendulumMonsterCard)
                .WithOne(b => b.MonsterCard)
                .HasForeignKey<PendulumMonsterCard>(c => c.MonsterCardId);

            modelBuilder.Entity<Card>()
                .HasOne(a => a.Archetype)
                .WithMany(b => b.Cards);

            modelBuilder.Entity<Banlist>()
                .HasMany(x => x.Statistics)
                .WithOne(y => y.Banlist);

            modelBuilder.Entity<Decklist>()
                .HasOne(x => x.Archetype)
                .WithMany(y => y.Decklists);

            modelBuilder.Entity<Decklist>()
                .HasMany(x => x.DecklistStatistics)
                .WithOne(y => y.Decklist);

            base.OnModelCreating(modelBuilder);
        }
    }
}
