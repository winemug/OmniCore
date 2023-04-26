﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OmniCore.Common.Data;

#nullable disable

namespace OmniCore.Common.Migrations
{
    [DbContext(typeof(OcdbContext))]
    partial class OcdbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("OmniCore.Common.Data.Account", b =>
                {
                    b.Property<Guid>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Country")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSynced")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .HasColumnType("TEXT");

                    b.HasKey("AccountId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("OmniCore.Common.Data.Client", b =>
                {
                    b.Property<Guid>("ClientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsSynced")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ClientId");

                    b.HasIndex("AccountId");

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("OmniCore.Common.Data.Pod", b =>
                {
                    b.Property<Guid>("PodId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ClientId")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSynced")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<uint?>("Lot")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Medication")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ProfileId")
                        .HasColumnType("TEXT");

                    b.Property<uint>("RadioAddress")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("Removed")
                        .HasColumnType("TEXT");

                    b.Property<uint?>("Serial")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UnitsPerMilliliter")
                        .HasColumnType("INTEGER");

                    b.HasKey("PodId");

                    b.ToTable("Pods");
                });

            modelBuilder.Entity("OmniCore.Common.Data.PodAction", b =>
                {
                    b.Property<Guid>("PodId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Index")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ClientId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSynced")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("ReceivedData")
                        .HasColumnType("BLOB");

                    b.Property<DateTimeOffset?>("RequestSentEarliest")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("RequestSentLatest")
                        .HasColumnType("TEXT");

                    b.Property<int>("Result")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("SentData")
                        .HasColumnType("BLOB");

                    b.HasKey("PodId", "Index");

                    b.ToTable("PodActions");
                });

            modelBuilder.Entity("OmniCore.Common.Data.Profile", b =>
                {
                    b.Property<Guid>("ProfileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsSynced")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ProfileId");

                    b.HasIndex("AccountId");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("OmniCore.Common.Data.Client", b =>
                {
                    b.HasOne("OmniCore.Common.Data.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("OmniCore.Common.Data.PodAction", b =>
                {
                    b.HasOne("OmniCore.Common.Data.Pod", null)
                        .WithMany("Actions")
                        .HasForeignKey("PodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OmniCore.Common.Data.Profile", b =>
                {
                    b.HasOne("OmniCore.Common.Data.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("OmniCore.Common.Data.Pod", b =>
                {
                    b.Navigation("Actions");
                });
#pragma warning restore 612, 618
        }
    }
}
