﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WFDS.Database;

#nullable disable

namespace WFDS.Database.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20241213015042_AddPlayerLogTable")]
    partial class AddPlayerLogTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("WFDS.Database.DbSet.BannedPlayer", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("BannedAt")
                        .HasColumnType("TEXT")
                        .HasColumnName("banned_at");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT")
                        .HasColumnName("display_name");

                    b.Property<ulong>("SteamId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("steam_id");

                    b.HasKey("Id");

                    b.HasIndex("SteamId")
                        .IsUnique();

                    b.ToTable("banned_players", (string)null);
                });

            modelBuilder.Entity("WFDS.Database.DbSet.ChatHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT")
                        .HasColumnName("created_at");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT")
                        .HasColumnName("display_name");

                    b.Property<bool>("IsLocal")
                        .HasColumnType("INTEGER")
                        .HasColumnName("is_local");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("message");

                    b.Property<ulong>("PlayerId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("player_id");

                    b.Property<float>("PositionX")
                        .HasColumnType("REAL")
                        .HasColumnName("position_x");

                    b.Property<float>("PositionY")
                        .HasColumnType("REAL")
                        .HasColumnName("position_y");

                    b.Property<float>("PositionZ")
                        .HasColumnType("REAL")
                        .HasColumnName("position_z");

                    b.Property<string>("Zone")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT")
                        .HasColumnName("zone");

                    b.Property<long>("ZoneOwner")
                        .HasColumnType("INTEGER")
                        .HasColumnName("zone_owner");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("PlayerId");

                    b.HasIndex("Zone", "ZoneOwner");

                    b.ToTable("chat_histories", (string)null);
                });

            modelBuilder.Entity("WFDS.Database.DbSet.Player", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT")
                        .HasColumnName("created_at");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT")
                        .HasColumnName("display_name");

                    b.Property<DateTimeOffset>("LastJoinedAt")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_joined_at");

                    b.Property<ulong>("SteamId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("steam_id");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("TEXT")
                        .HasColumnName("updated_at");

                    b.HasKey("Id");

                    b.HasIndex("SteamId")
                        .IsUnique();

                    b.ToTable("players", (string)null);
                });

            modelBuilder.Entity("WFDS.Database.DbSet.PlayerLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT")
                        .HasColumnName("action");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT")
                        .HasColumnName("created_at");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT")
                        .HasColumnName("display_name");

                    b.Property<string>("JsonData")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("json_data");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("message");

                    b.Property<ulong>("PlayerId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("player_id");

                    b.Property<float>("PositionX")
                        .HasColumnType("REAL")
                        .HasColumnName("position_x");

                    b.Property<float>("PositionY")
                        .HasColumnType("REAL")
                        .HasColumnName("position_y");

                    b.Property<float>("PositionZ")
                        .HasColumnType("REAL")
                        .HasColumnName("position_z");

                    b.Property<string>("Zone")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT")
                        .HasColumnName("zone");

                    b.Property<long>("ZoneOwner")
                        .HasColumnType("INTEGER")
                        .HasColumnName("zone_owner");

                    b.HasKey("Id");

                    b.ToTable("player_logs", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
