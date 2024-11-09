﻿// <auto-generated />
using System;
using System.Collections.Generic;
using HabitTracker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HabitTracker.Data.Migrations
{
    [DbContext(typeof(HabitTrackerContext))]
    partial class HabitTrackerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("HabitTracker.Data.HabitEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateOnly>("CreationDate")
                        .HasColumnType("date");

                    b.Property<DateTime?>("ExpirationDate")
                        .HasColumnType("timestamp");

                    b.Property<bool>("IsNecessary")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsPaused")
                        .HasColumnType("boolean");

                    b.Property<DateOnly?>("LastExecutionDate")
                        .HasColumnType("date");

                    b.Property<int>("NumberOfExecutions")
                        .HasColumnType("integer");

                    b.Property<long>("ProgressDays")
                        .HasColumnType("bigint");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Habits");
                });

            modelBuilder.Entity("HabitTracker.Data.Models.HabitNotificationEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("CountOfNotifications")
                        .HasColumnType("integer");

                    b.Property<Guid>("HabitId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsSending")
                        .HasColumnType("boolean");

                    b.Property<List<TimeOnly>>("NotificationTime")
                        .IsRequired()
                        .HasColumnType("time without time zone[]");

                    b.Property<Guid>("UserNotificationsId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("HabitId");

                    b.HasIndex("UserNotificationsId");

                    b.ToTable("HabitsNotificationSettings");
                });

            modelBuilder.Entity("HabitTracker.Data.Models.NotificationEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<TimeOnly>("TimeEnd")
                        .HasColumnType("time without time zone");

                    b.Property<TimeOnly>("TimeStart")
                        .HasColumnType("time without time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("NotificationSettings");
                });

            modelBuilder.Entity("HabitTracker.Data.Models.PracticedHabitEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("HabitId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("LastExecutionDate")
                        .HasColumnType("timestamp");

                    b.HasKey("Id");

                    b.HasIndex("HabitId");

                    b.ToTable("PracticedHabits");
                });

            modelBuilder.Entity("HabitTracker.Data.UserEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("HabitTracker.Data.HabitEntity", b =>
                {
                    b.HasOne("HabitTracker.Data.UserEntity", "User")
                        .WithMany("Habbits")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("HabitTracker.Data.Models.HabitNotificationEntity", b =>
                {
                    b.HasOne("HabitTracker.Data.HabitEntity", "Habit")
                        .WithMany()
                        .HasForeignKey("HabitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("HabitTracker.Data.Models.NotificationEntity", "Notification")
                        .WithMany("HabitNotifications")
                        .HasForeignKey("UserNotificationsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Habit");

                    b.Navigation("Notification");
                });

            modelBuilder.Entity("HabitTracker.Data.Models.NotificationEntity", b =>
                {
                    b.HasOne("HabitTracker.Data.UserEntity", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("HabitTracker.Data.Models.PracticedHabitEntity", b =>
                {
                    b.HasOne("HabitTracker.Data.HabitEntity", "Habit")
                        .WithMany()
                        .HasForeignKey("HabitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Habit");
                });

            modelBuilder.Entity("HabitTracker.Data.Models.NotificationEntity", b =>
                {
                    b.Navigation("HabitNotifications");
                });

            modelBuilder.Entity("HabitTracker.Data.UserEntity", b =>
                {
                    b.Navigation("Habbits");
                });
#pragma warning restore 612, 618
        }
    }
}
