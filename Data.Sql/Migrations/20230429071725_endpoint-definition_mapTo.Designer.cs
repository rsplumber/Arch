﻿// <auto-generated />
using System;
using Data.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.Sql.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230429071725_endpoint-definition_mapTo")]
    partial class endpointdefinition_mapTo
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Core.EndpointDefinitions.EndpointDefinition", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Endpoint")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("endpoint");

                    b.Property<string>("MapTo")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("map_to");

                    b.Property<string>("Method")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("method");

                    b.Property<string>("Pattern")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("pattern");

                    b.Property<Guid>("service_config_id")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("Endpoint");

                    b.HasIndex("MapTo");

                    b.HasIndex("Pattern");

                    b.HasIndex("service_config_id");

                    b.ToTable("endpoint_definitions", (string)null);
                });

            modelBuilder.Entity("Core.Metas.Meta", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("key");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("value");

                    b.Property<Guid?>("endpoint_definition_id")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("service_config_id")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("Key");

                    b.HasIndex("endpoint_definition_id");

                    b.HasIndex("service_config_id");

                    b.ToTable("meta", (string)null);
                });

            modelBuilder.Entity("Core.ServiceConfigs.ServiceConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("BaseUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("base_url");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at_utc");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<bool>("Primary")
                        .HasColumnType("boolean")
                        .HasColumnName("primary");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("service_configs", (string)null);
                });

            modelBuilder.Entity("Core.EndpointDefinitions.EndpointDefinition", b =>
                {
                    b.HasOne("Core.ServiceConfigs.ServiceConfig", "ServiceConfig")
                        .WithMany("EndpointDefinitions")
                        .HasForeignKey("service_config_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ServiceConfig");
                });

            modelBuilder.Entity("Core.Metas.Meta", b =>
                {
                    b.HasOne("Core.EndpointDefinitions.EndpointDefinition", "EndpointDefinition")
                        .WithMany("Meta")
                        .HasForeignKey("endpoint_definition_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Core.ServiceConfigs.ServiceConfig", "ServiceConfig")
                        .WithMany("Meta")
                        .HasForeignKey("service_config_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("EndpointDefinition");

                    b.Navigation("ServiceConfig");
                });

            modelBuilder.Entity("Core.EndpointDefinitions.EndpointDefinition", b =>
                {
                    b.Navigation("Meta");
                });

            modelBuilder.Entity("Core.ServiceConfigs.ServiceConfig", b =>
                {
                    b.Navigation("EndpointDefinitions");

                    b.Navigation("Meta");
                });
#pragma warning restore 612, 618
        }
    }
}
