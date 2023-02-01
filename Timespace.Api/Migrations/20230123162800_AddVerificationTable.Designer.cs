﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence;

#nullable disable

namespace Timespace.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230123162800_AddVerificationTable")]
    partial class AddVerificationTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Timespace.Api.Application.Features.Authentication.Login.Common.Entities.LoginFlow", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<List<string>>("AllowedMethodsForNextStep")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Instant>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("IdentityId")
                        .HasColumnType("uuid");

                    b.Property<string>("NextStep")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("RememberMe")
                        .HasColumnType("boolean");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("IdentityId");

                    b.HasIndex("TenantId");

                    b.ToTable("LoginFlows");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Authentication.Registration.Common.Entities.RegistrationFlow", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CompanyIndustry")
                        .HasColumnType("text");

                    b.Property<string>("CompanyName")
                        .HasColumnType("text");

                    b.Property<int?>("CompanySize")
                        .HasColumnType("integer");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CredentialType")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Instant>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<string>("NextStep")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("RegistrationFlows");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Authentication.Sessions.Common.Entities.Session", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Instant>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("IdentityId")
                        .HasColumnType("uuid");

                    b.Property<string>("SessionToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("IdentityId");

                    b.HasIndex("TenantId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Tenants.Common.Entities.Tenant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CompanyIndustry")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("CompanySize")
                        .HasColumnType("integer");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Instant?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Common.Entities.Credentials.IdentityCredential", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Configuration")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CredentialType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("IdentityId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.HasIndex("IdentityId", "CredentialType")
                        .IsUnique();

                    b.ToTable("IdentityCredentials");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Common.Entities.Identity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Instant?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("RequiresMfa")
                        .HasColumnType("boolean");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("Identities");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Common.Entities.IdentityIdentifier", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("AllowLogin")
                        .HasColumnType("boolean");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Instant?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("IdentityId")
                        .HasColumnType("uuid");

                    b.Property<Instant>("LastVerificationRequestSent")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Primary")
                        .HasColumnType("boolean");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Verified")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("IdentityId");

                    b.HasIndex("TenantId");

                    b.ToTable("IdentityIdentifiers");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Common.Entities.IdentityLookupSecret", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("IdentityId")
                        .HasColumnType("uuid");

                    b.Property<List<LookupSecret>>("Secrets")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("IdentityId")
                        .IsUnique();

                    b.HasIndex("TenantId");

                    b.ToTable("IdentityLookupSecret");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Settings.Mfa.Entities.MfaSetupFlow", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Instant>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("IdentityId")
                        .HasColumnType("uuid");

                    b.Property<string>("Secret")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("IdentityId");

                    b.HasIndex("TenantId");

                    b.ToTable("MfaSetupFlows");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Authentication.Login.Common.Entities.LoginFlow", b =>
                {
                    b.HasOne("Timespace.Api.Application.Features.Users.Common.Entities.Identity", "Identity")
                        .WithMany()
                        .HasForeignKey("IdentityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Timespace.Api.Application.Features.Tenants.Common.Entities.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Identity");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Authentication.Sessions.Common.Entities.Session", b =>
                {
                    b.HasOne("Timespace.Api.Application.Features.Users.Common.Entities.Identity", "Identity")
                        .WithMany()
                        .HasForeignKey("IdentityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Timespace.Api.Application.Features.Tenants.Common.Entities.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Identity");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Common.Entities.Credentials.IdentityCredential", b =>
                {
                    b.HasOne("Timespace.Api.Application.Features.Users.Common.Entities.Identity", "Identity")
                        .WithMany("Credentials")
                        .HasForeignKey("IdentityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Timespace.Api.Application.Features.Tenants.Common.Entities.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Identity");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Common.Entities.Identity", b =>
                {
                    b.HasOne("Timespace.Api.Application.Features.Tenants.Common.Entities.Tenant", "Tenant")
                        .WithMany("Members")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Common.Entities.IdentityIdentifier", b =>
                {
                    b.HasOne("Timespace.Api.Application.Features.Users.Common.Entities.Identity", "Identity")
                        .WithMany("Identifiers")
                        .HasForeignKey("IdentityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Timespace.Api.Application.Features.Tenants.Common.Entities.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Identity");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Common.Entities.IdentityLookupSecret", b =>
                {
                    b.HasOne("Timespace.Api.Application.Features.Users.Common.Entities.Identity", "Identity")
                        .WithMany()
                        .HasForeignKey("IdentityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Timespace.Api.Application.Features.Tenants.Common.Entities.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Identity");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Settings.Mfa.Entities.MfaSetupFlow", b =>
                {
                    b.HasOne("Timespace.Api.Application.Features.Users.Common.Entities.Identity", "Identity")
                        .WithMany()
                        .HasForeignKey("IdentityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Timespace.Api.Application.Features.Tenants.Common.Entities.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Identity");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Tenants.Common.Entities.Tenant", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("Timespace.Api.Application.Features.Users.Common.Entities.Identity", b =>
                {
                    b.Navigation("Credentials");

                    b.Navigation("Identifiers");
                });
#pragma warning restore 612, 618
        }
    }
}
