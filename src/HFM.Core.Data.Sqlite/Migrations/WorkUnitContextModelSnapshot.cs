﻿// <auto-generated />
using System;
using HFM.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace HFM.Core.Migrations
{
    [DbContext(typeof(WorkUnitContext))]
    partial class WorkUnitContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.4");

            modelBuilder.Entity("HFM.Core.Data.ClientEntity", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConnectionString")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Guid")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("HFM.Core.Data.PlatformEntity", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CUDAVersion")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClientVersion")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ComputeVersion")
                        .HasColumnType("TEXT");

                    b.Property<string>("DriverVersion")
                        .HasColumnType("TEXT");

                    b.Property<string>("Implementation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OperatingSystem")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Processor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("Threads")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("Platforms");
                });

            modelBuilder.Entity("HFM.Core.Data.ProteinEntity", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Atoms")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Core")
                        .HasColumnType("TEXT");

                    b.Property<double>("Credit")
                        .HasColumnType("REAL");

                    b.Property<double>("ExpirationDays")
                        .HasColumnType("REAL");

                    b.Property<int>("Frames")
                        .HasColumnType("INTEGER");

                    b.Property<double>("KFactor")
                        .HasColumnType("REAL");

                    b.Property<int>("ProjectID")
                        .HasColumnType("INTEGER");

                    b.Property<double>("TimeoutDays")
                        .HasColumnType("REAL");

                    b.HasKey("ID");

                    b.ToTable("Proteins");
                });

            modelBuilder.Entity("HFM.Core.Data.VersionEntity", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Versions");
                });

            modelBuilder.Entity("HFM.Core.Data.WorkUnitEntity", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Assigned")
                        .HasColumnType("TEXT");

                    b.Property<long>("ClientID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ClientSlot")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CoreVersion")
                        .HasColumnType("TEXT");

                    b.Property<string>("DonorName")
                        .HasColumnType("TEXT");

                    b.Property<int>("DonorTeam")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("Finished")
                        .HasColumnType("TEXT");

                    b.Property<int?>("FrameTimeInSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("FramesCompleted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("HexID")
                        .HasColumnType("TEXT");

                    b.Property<long?>("PlatformID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectClone")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectGen")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectRun")
                        .HasColumnType("INTEGER");

                    b.Property<long>("ProteinID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Result")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ClientID");

                    b.HasIndex("PlatformID");

                    b.HasIndex("ProteinID");

                    b.ToTable("WorkUnits");
                });

            modelBuilder.Entity("HFM.Core.Data.WorkUnitFrameEntity", b =>
                {
                    b.Property<long>("WorkUnitID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FrameID")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<int>("RawFramesComplete")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RawFramesTotal")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("TimeStamp")
                        .HasColumnType("TEXT");

                    b.HasKey("WorkUnitID", "FrameID");

                    b.ToTable("WorkUnitFrames");
                });

            modelBuilder.Entity("HFM.Core.Data.WorkUnitEntity", b =>
                {
                    b.HasOne("HFM.Core.Data.ClientEntity", "Client")
                        .WithMany()
                        .HasForeignKey("ClientID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("HFM.Core.Data.PlatformEntity", "Platform")
                        .WithMany()
                        .HasForeignKey("PlatformID");

                    b.HasOne("HFM.Core.Data.ProteinEntity", "Protein")
                        .WithMany()
                        .HasForeignKey("ProteinID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Client");

                    b.Navigation("Platform");

                    b.Navigation("Protein");
                });

            modelBuilder.Entity("HFM.Core.Data.WorkUnitFrameEntity", b =>
                {
                    b.HasOne("HFM.Core.Data.WorkUnitEntity", null)
                        .WithMany("Frames")
                        .HasForeignKey("WorkUnitID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("HFM.Core.Data.WorkUnitEntity", b =>
                {
                    b.Navigation("Frames");
                });
#pragma warning restore 612, 618
        }
    }
}