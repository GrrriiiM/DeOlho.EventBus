﻿// <auto-generated />
using System;
using DeOlho.EventBus.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DeOlho.EventBus.EventSourcing.Migrations
{
    [DbContext(typeof(EventSourcingDbContext))]
    [Migration("20190819232417_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("DeOlho.EventBus.EventSourcing.EventLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AssemblyName");

                    b.Property<byte[]>("Content");

                    b.Property<DateTime>("DateTimeCreation");

                    b.Property<int>("Status");

                    b.Property<string>("TypeName");

                    b.HasKey("Id");

                    b.ToTable("EventLogs");
                });
#pragma warning restore 612, 618
        }
    }
}