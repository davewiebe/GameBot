﻿// <auto-generated />
using System;
using PerudoBot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace KarmaBot.Migrations
{
    [DbContext(typeof(GameBotDbContext))]
    partial class GameBotDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("GameBot.Data.Karma", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("FromUserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("GivenOn")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Points")
                        .HasColumnType("integer");

                    b.Property<decimal>("Server")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Thing")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Karma");
                });

            modelBuilder.Entity("GameBot.Data.KeyPhrase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("KeyPhrase");
                });

            modelBuilder.Entity("GameBot.Data.Phrase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("KeyPhraseId")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("KeyPhraseId");

                    b.ToTable("Phrase");
                });

            modelBuilder.Entity("GameBot.Data.Score", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Points")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Scores");
                });

            modelBuilder.Entity("GameBot.Data.Phrase", b =>
                {
                    b.HasOne("GameBot.Data.KeyPhrase", "KeyPhrase")
                        .WithMany("Phrases")
                        .HasForeignKey("KeyPhraseId");
                });
#pragma warning restore 612, 618
        }
    }
}
