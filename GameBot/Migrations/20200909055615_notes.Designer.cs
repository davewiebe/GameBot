﻿// <auto-generated />
using System;
using GameBot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GameBot.Migrations
{
    [DbContext(typeof(GameBotDbContext))]
    [Migration("20200909055615_notes")]
    partial class notes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("GameBot.Data.Bid", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Call")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GameId")
                        .HasColumnType("int");

                    b.Property<int>("Pips")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("Bids");
                });

            modelBuilder.Entity("GameBot.Data.BotKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BotAesKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("BotKeys");
                });

            modelBuilder.Entity("GameBot.Data.DeepThought", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("PostId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("DeepThoughts");
                });

            modelBuilder.Entity("GameBot.Data.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("CanBidAnytime")
                        .HasColumnType("bit");

                    b.Property<bool>("CanCallExactAnytime")
                        .HasColumnType("bit");

                    b.Property<bool>("CanCallLiarAnytime")
                        .HasColumnType("bit");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<int>("ExactCallBonus")
                        .HasColumnType("int");

                    b.Property<int>("ExactCallPenalty")
                        .HasColumnType("int");

                    b.Property<bool>("FaceoffEnabled")
                        .HasColumnType("bit");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<bool>("IsRanked")
                        .HasColumnType("bit");

                    b.Property<bool>("NextRoundIsPalifico")
                        .HasColumnType("bit");

                    b.Property<int>("NumberOfDice")
                        .HasColumnType("int");

                    b.Property<bool>("Palifico")
                        .HasColumnType("bit");

                    b.Property<int>("Penalty")
                        .HasColumnType("int");

                    b.Property<int?>("PlayerTurnId")
                        .HasColumnType("int");

                    b.Property<bool>("RandomizeBetweenRounds")
                        .HasColumnType("bit");

                    b.Property<int>("RoundStartPlayerId")
                        .HasColumnType("int");

                    b.Property<int>("State")
                        .HasColumnType("int");

                    b.Property<bool>("WildsEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("Winner")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("GameBot.Data.Karma", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("FromUserId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<DateTime>("GivenOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("Points")
                        .HasColumnType("int");

                    b.Property<decimal>("Server")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Thing")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Karma");
                });

            modelBuilder.Entity("GameBot.Data.KeyPhrase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("KeyPhrase");
                });

            modelBuilder.Entity("GameBot.Data.Note", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("GameId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Notes");
                });

            modelBuilder.Entity("GameBot.Data.Phrase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("KeyPhraseId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("KeyPhraseId");

                    b.ToTable("Phrase");
                });

            modelBuilder.Entity("GameBot.Data.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Dice")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GameId")
                        .HasColumnType("int");

                    b.Property<bool>("IsBot")
                        .HasColumnType("bit");

                    b.Property<int>("NumberOfDice")
                        .HasColumnType("int");

                    b.Property<int>("TurnOrder")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("GameBot.Data.Score", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Points")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Scores");
                });

            modelBuilder.Entity("GameBot.Data.Bid", b =>
                {
                    b.HasOne("GameBot.Data.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GameBot.Data.Note", b =>
                {
                    b.HasOne("GameBot.Data.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GameBot.Data.Phrase", b =>
                {
                    b.HasOne("GameBot.Data.KeyPhrase", "KeyPhrase")
                        .WithMany("Phrases")
                        .HasForeignKey("KeyPhraseId");
                });

            modelBuilder.Entity("GameBot.Data.Player", b =>
                {
                    b.HasOne("GameBot.Data.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
