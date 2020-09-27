﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PerudoBot.Data;

namespace PerudoBot.Migrations
{
    [DbContext(typeof(GameBotDbContext))]
    [Migration("20200927045216_GhostExactCalls")]
    partial class GhostExactCalls
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("PerudoBot.Data.Bid", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Call")
                        .HasColumnType("text");

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<int>("Pips")
                        .HasColumnType("integer");

                    b.Property<int>("PlayerId")
                        .HasColumnType("integer");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("Bids");
                });

            modelBuilder.Entity("PerudoBot.Data.BotKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("BotAesKey")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("BotKeys");
                });

            modelBuilder.Entity("PerudoBot.Data.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("CanBidAnytime")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanCallExactAnytime")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanCallExactToJoinAgain")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanCallLiarAnytime")
                        .HasColumnType("boolean");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("DateFinished")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("ExactCallBonus")
                        .HasColumnType("integer");

                    b.Property<int>("ExactCallPenalty")
                        .HasColumnType("integer");

                    b.Property<bool>("FaceoffEnabled")
                        .HasColumnType("boolean");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IsRanked")
                        .HasColumnType("boolean");

                    b.Property<bool>("NextRoundIsPalifico")
                        .HasColumnType("boolean");

                    b.Property<int>("NumberOfDice")
                        .HasColumnType("integer");

                    b.Property<bool>("Palifico")
                        .HasColumnType("boolean");

                    b.Property<int>("Penalty")
                        .HasColumnType("integer");

                    b.Property<int?>("PlayerTurnId")
                        .HasColumnType("integer");

                    b.Property<bool>("RandomizeBetweenRounds")
                        .HasColumnType("boolean");

                    b.Property<int>("RoundStartPlayerId")
                        .HasColumnType("integer");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<decimal>("StatusMessage")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("WildsEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("Winner")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("PerudoBot.Data.Note", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Notes");
                });

            modelBuilder.Entity("PerudoBot.Data.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Dice")
                        .HasColumnType("text");

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<int>("GhostAttemptPips")
                        .HasColumnType("integer");

                    b.Property<int>("GhostAttemptQuantity")
                        .HasColumnType("integer");

                    b.Property<int>("GhostAttemptsLeft")
                        .HasColumnType("integer");

                    b.Property<bool>("IsBot")
                        .HasColumnType("boolean");

                    b.Property<int>("NumberOfDice")
                        .HasColumnType("integer");

                    b.Property<int>("TurnOrder")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("PerudoBot.Data.Rattle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Deathrattle")
                        .HasColumnType("text");

                    b.Property<string>("Tauntrattle")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.Property<string>("Winrattle")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Rattles");
                });

            modelBuilder.Entity("PerudoBot.Data.Bid", b =>
                {
                    b.HasOne("PerudoBot.Data.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PerudoBot.Data.Note", b =>
                {
                    b.HasOne("PerudoBot.Data.Game", "Game")
                        .WithMany("Notes")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PerudoBot.Data.Player", b =>
                {
                    b.HasOne("PerudoBot.Data.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
