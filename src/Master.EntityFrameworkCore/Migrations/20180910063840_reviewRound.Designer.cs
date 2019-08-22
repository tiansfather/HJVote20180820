﻿// <auto-generated />
using System;
using Master.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Master.Migrations
{
    [DbContext(typeof(MasterDbContext))]
    [Migration("20180910063840_reviewRound")]
    partial class reviewRound
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Master.Auditing.AuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BrowserInfo");

                    b.Property<string>("ClientIpAddress");

                    b.Property<string>("ClientName");

                    b.Property<string>("CustomData");

                    b.Property<string>("Exception");

                    b.Property<int>("ExecutionDuration");

                    b.Property<DateTime>("ExecutionTime");

                    b.Property<int?>("ImpersonatorTenantId");

                    b.Property<long?>("ImpersonatorUserId");

                    b.Property<string>("MethodName");

                    b.Property<string>("Parameters");

                    b.Property<string>("ServiceName");

                    b.Property<int?>("TenantId");

                    b.Property<long?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("TenantId", "ExecutionDuration");

                    b.HasIndex("TenantId", "ExecutionTime");

                    b.HasIndex("TenantId", "UserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("Master.Authentication.PermissionSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<bool>("IsGranted");

                    b.Property<string>("Name");

                    b.Property<int?>("TenantId");

                    b.HasKey("Id");

                    b.HasIndex("TenantId", "Name");

                    b.ToTable("Permissions");

                    b.HasDiscriminator<string>("Discriminator").HasValue("PermissionSetting");
                });

            modelBuilder.Entity("Master.Authentication.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<long?>("DeleterUserId");

                    b.Property<DateTime?>("DeletionTime");

                    b.Property<string>("DisplayName");

                    b.Property<bool>("IsDefault");

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsStatic");

                    b.Property<DateTime?>("LastModificationTime");

                    b.Property<long?>("LastModifierUserId");

                    b.Property<string>("Name");

                    b.Property<int?>("TenantId");

                    b.HasKey("Id");

                    b.HasIndex("CreatorUserId");

                    b.HasIndex("DeleterUserId");

                    b.HasIndex("LastModifierUserId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Master.Authentication.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<long?>("DeleterUserId");

                    b.Property<DateTime?>("DeletionTime");

                    b.Property<string>("ExtensionData");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime?>("LastLoginTime");

                    b.Property<DateTime?>("LastModificationTime");

                    b.Property<long?>("LastModifierUserId");

                    b.Property<string>("Name");

                    b.Property<int?>("OrganizationId");

                    b.Property<string>("Password");

                    b.Property<string>("PhoneNumber");

                    b.Property<int?>("TenantId");

                    b.Property<string>("UserName");

                    b.HasKey("Id");

                    b.HasIndex("CreatorUserId");

                    b.HasIndex("DeleterUserId");

                    b.HasIndex("LastModifierUserId");

                    b.HasIndex("OrganizationId");

                    b.HasIndex("TenantId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Master.Authentication.UserLoginAttempt", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BrowserInfo");

                    b.Property<string>("ClientIpAddress");

                    b.Property<string>("ClientName");

                    b.Property<DateTime>("CreationTime");

                    b.Property<byte>("Result");

                    b.Property<string>("TenancyName");

                    b.Property<int?>("TenantId");

                    b.Property<long?>("UserId");

                    b.Property<string>("UserNameOrPhoneNumber");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "TenantId");

                    b.HasIndex("TenancyName", "UserNameOrPhoneNumber", "Result");

                    b.ToTable("UserLoginAttempts");
                });

            modelBuilder.Entity("Master.Authentication.UserRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<int>("RoleId");

                    b.Property<int?>("TenantId");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("TenantId", "RoleId");

                    b.HasIndex("TenantId", "UserId");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("Master.Configuration.Setting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<DateTime?>("LastModificationTime");

                    b.Property<long?>("LastModifierUserId");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int?>("TenantId");

                    b.Property<long?>("UserId");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("TenantId", "Name");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("Master.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<long?>("DeleterUserId");

                    b.Property<DateTime?>("DeletionTime");

                    b.Property<string>("ExtensionData");

                    b.Property<string>("FileName");

                    b.Property<string>("FilePath");

                    b.Property<decimal>("FileSize");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime?>("LastModificationTime");

                    b.Property<long?>("LastModifierUserId");

                    b.Property<string>("Remarks");

                    b.Property<int?>("TenantId");

                    b.HasKey("Id");

                    b.HasIndex("CreatorUserId");

                    b.HasIndex("DeleterUserId");

                    b.HasIndex("LastModifierUserId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Master.Majors.Major", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BriefCode");

                    b.Property<string>("BriefName");

                    b.Property<string>("Code");

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("DisplayName");

                    b.Property<string>("ExtensionData");

                    b.Property<bool>("IsActive");

                    b.Property<int?>("MatchId");

                    b.Property<int?>("MatchInstanceId");

                    b.Property<int?>("ParentId");

                    b.Property<string>("Remarks");

                    b.Property<int>("Sort");

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.HasIndex("MatchInstanceId");

                    b.HasIndex("ParentId");

                    b.ToTable("Majors");
                });

            modelBuilder.Entity("Master.Majors.MajorExpert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<int>("MajorExpertRank");

                    b.Property<int>("MajorId");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.ToTable("MajorExperts");
                });

            modelBuilder.Entity("Master.Matches.Match", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<long?>("DeleterUserId");

                    b.Property<DateTime?>("DeletionTime");

                    b.Property<string>("ExtensionData");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime?>("LastModificationTime");

                    b.Property<long?>("LastModifierUserId");

                    b.Property<string>("Name");

                    b.Property<string>("Remarks");

                    b.HasKey("Id");

                    b.HasIndex("CreatorUserId");

                    b.HasIndex("DeleterUserId");

                    b.HasIndex("LastModifierUserId");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("Master.Matches.MatchInstance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<long?>("DeleterUserId");

                    b.Property<DateTime?>("DeletionTime");

                    b.Property<string>("ExtensionData");

                    b.Property<string>("Identifier");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime?>("LastModificationTime");

                    b.Property<long?>("LastModifierUserId");

                    b.Property<int>("MatchId");

                    b.Property<int>("MatchInstanceStatus");

                    b.Property<string>("Remarks");

                    b.Property<int>("Year");

                    b.HasKey("Id");

                    b.HasIndex("CreatorUserId");

                    b.HasIndex("DeleterUserId");

                    b.HasIndex("LastModifierUserId");

                    b.HasIndex("MatchId");

                    b.ToTable("MatchInstances");
                });

            modelBuilder.Entity("Master.Matches.MatchResource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("ExtensionData");

                    b.Property<int>("MajorId");

                    b.Property<int?>("MatchId");

                    b.Property<int?>("MatchInstanceId");

                    b.Property<int>("MatchResourceStatus");

                    b.Property<int>("MatchResourceType");

                    b.Property<int?>("SubMajorId");

                    b.HasKey("Id");

                    b.HasIndex("MajorId");

                    b.HasIndex("MatchId");

                    b.HasIndex("MatchInstanceId");

                    b.HasIndex("SubMajorId");

                    b.ToTable("MatchResources");
                });

            modelBuilder.Entity("Master.MultiTenancy.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ConnectionString");

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<long?>("DeleterUserId");

                    b.Property<DateTime?>("DeletionTime");

                    b.Property<int?>("EditionId");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime?>("LastModificationTime");

                    b.Property<long?>("LastModifierUserId");

                    b.Property<string>("Name");

                    b.Property<string>("TenancyName");

                    b.HasKey("Id");

                    b.HasIndex("CreatorUserId");

                    b.HasIndex("DeleterUserId");

                    b.HasIndex("LastModifierUserId");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("Master.Notices.Notice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Content");

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("ExtensionData");

                    b.Property<DateTime?>("LastModificationTime");

                    b.Property<long?>("LastModifierUserId");

                    b.Property<int>("NoticeStatus");

                    b.Property<DateTime?>("PublishTime");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.ToTable("Notices");
                });

            modelBuilder.Entity("Master.Organizations.Organization", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BriefCode");

                    b.Property<string>("BriefName");

                    b.Property<string>("Code");

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<long?>("DeleterUserId");

                    b.Property<DateTime?>("DeletionTime");

                    b.Property<string>("DisplayName")
                        .IsRequired();

                    b.Property<string>("ExtensionData");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime?>("LastModificationTime");

                    b.Property<long?>("LastModifierUserId");

                    b.Property<int?>("ParentId");

                    b.Property<string>("Remarks");

                    b.Property<int>("Sort");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("Organizations");
                });

            modelBuilder.Entity("Master.Prizes.Prize", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("ExtensionData");

                    b.Property<bool>("IsActive");

                    b.Property<int>("MajorId");

                    b.Property<int?>("MatchId");

                    b.Property<int?>("MatchInstanceId");

                    b.Property<string>("PrizeName");

                    b.Property<int>("PrizeType");

                    b.Property<string>("Remarks");

                    b.HasKey("Id");

                    b.HasIndex("MajorId");

                    b.HasIndex("MatchId");

                    b.HasIndex("MatchInstanceId");

                    b.ToTable("Prizes");
                });

            modelBuilder.Entity("Master.Prizes.PrizeSubMajor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Checked");

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<int>("MajorId");

                    b.Property<int?>("Percent");

                    b.Property<int>("PrizeId");

                    b.HasKey("Id");

                    b.HasIndex("MajorId");

                    b.HasIndex("PrizeId");

                    b.ToTable("PrizeSubMajors");
                });

            modelBuilder.Entity("Master.Projects.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BuildingCity");

                    b.Property<string>("BuildingCompany");

                    b.Property<string>("BuildingCountry");

                    b.Property<string>("BuildingProvince");

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("DesignOrganizationContact");

                    b.Property<string>("DesignOrganizationEmail");

                    b.Property<int?>("DesignOrganizationId");

                    b.Property<string>("DesignOrganizationMobile");

                    b.Property<string>("DesignOrganizationPhone");

                    b.Property<string>("ExtensionData");

                    b.Property<bool>("HasCoorparation");

                    b.Property<bool>("IsOriginal");

                    b.Property<int>("MatchInstanceId");

                    b.Property<int>("PrizeId");

                    b.Property<int?>("PrizeSubMajorId");

                    b.Property<string>("ProjectName");

                    b.Property<string>("ProjectSN");

                    b.Property<int>("ProjectSource");

                    b.Property<int>("ProjectStatus");

                    b.Property<string>("ReportSN");

                    b.HasKey("Id");

                    b.HasIndex("CreatorUserId");

                    b.HasIndex("DesignOrganizationId");

                    b.HasIndex("MatchInstanceId");

                    b.HasIndex("PrizeId");

                    b.HasIndex("PrizeSubMajorId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("Master.Projects.ProjectMajorInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("ExtensionData");

                    b.Property<int?>("MajorId");

                    b.Property<int>("ProjectId");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("ProjectMajorInfos");
                });

            modelBuilder.Entity("Master.Projects.ProjectTraceLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ActionName");

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<int>("ProjectId");

                    b.Property<string>("Remarks");

                    b.Property<int>("TargetStatus");

                    b.HasKey("Id");

                    b.HasIndex("CreatorUserId");

                    b.HasIndex("ProjectId");

                    b.ToTable("ProjectTraceLogs");
                });

            modelBuilder.Entity("Master.Reviews.Review", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("ExtensionData");

                    b.Property<int>("MajorId");

                    b.Property<int>("MatchInstanceId");

                    b.Property<string>("Remarks");

                    b.Property<string>("ReviewMajorName");

                    b.Property<string>("ReviewName");

                    b.Property<int>("ReviewStatus");

                    b.Property<int>("ReviewType");

                    b.Property<DateTime?>("StartTime");

                    b.Property<int?>("SubMajorId");

                    b.HasKey("Id");

                    b.HasIndex("MajorId");

                    b.HasIndex("MatchInstanceId");

                    b.ToTable("Reviews");
                });

            modelBuilder.Entity("Master.Reviews.ReviewRound", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("ExtensionData");

                    b.Property<string>("ResultProjectIDs");

                    b.Property<int>("ReviewId");

                    b.Property<int>("ReviewMethod");

                    b.Property<int>("ReviewStatus");

                    b.Property<int>("Round");

                    b.Property<string>("SourceProjectIDs");

                    b.Property<int>("TargetNumber");

                    b.Property<int>("Turn");

                    b.HasKey("Id");

                    b.HasIndex("ReviewId");

                    b.ToTable("ReviewRounds");
                });

            modelBuilder.Entity("Master.Authentication.RolePermissionSetting", b =>
                {
                    b.HasBaseType("Master.Authentication.PermissionSetting");

                    b.Property<int>("RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("Permissions");

                    b.HasDiscriminator().HasValue("RolePermissionSetting");
                });

            modelBuilder.Entity("Master.Authentication.UserPermissionSetting", b =>
                {
                    b.HasBaseType("Master.Authentication.PermissionSetting");

                    b.Property<long>("UserId");

                    b.HasIndex("UserId");

                    b.ToTable("Permissions");

                    b.HasDiscriminator().HasValue("UserPermissionSetting");
                });

            modelBuilder.Entity("Master.Authentication.Role", b =>
                {
                    b.HasOne("Master.Authentication.User", "CreatorUser")
                        .WithMany()
                        .HasForeignKey("CreatorUserId");

                    b.HasOne("Master.Authentication.User", "DeleterUser")
                        .WithMany()
                        .HasForeignKey("DeleterUserId");

                    b.HasOne("Master.Authentication.User", "LastModifierUser")
                        .WithMany()
                        .HasForeignKey("LastModifierUserId");
                });

            modelBuilder.Entity("Master.Authentication.User", b =>
                {
                    b.HasOne("Master.Authentication.User", "CreatorUser")
                        .WithMany()
                        .HasForeignKey("CreatorUserId");

                    b.HasOne("Master.Authentication.User", "DeleterUser")
                        .WithMany()
                        .HasForeignKey("DeleterUserId");

                    b.HasOne("Master.Authentication.User", "LastModifierUser")
                        .WithMany()
                        .HasForeignKey("LastModifierUserId");

                    b.HasOne("Master.Organizations.Organization", "Organization")
                        .WithMany()
                        .HasForeignKey("OrganizationId");

                    b.HasOne("Master.MultiTenancy.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId");
                });

            modelBuilder.Entity("Master.Authentication.UserRole", b =>
                {
                    b.HasOne("Master.Authentication.User")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master.File", b =>
                {
                    b.HasOne("Master.Authentication.User", "CreatorUser")
                        .WithMany()
                        .HasForeignKey("CreatorUserId");

                    b.HasOne("Master.Authentication.User", "DeleterUser")
                        .WithMany()
                        .HasForeignKey("DeleterUserId");

                    b.HasOne("Master.Authentication.User", "LastModifierUser")
                        .WithMany()
                        .HasForeignKey("LastModifierUserId");
                });

            modelBuilder.Entity("Master.Majors.Major", b =>
                {
                    b.HasOne("Master.Matches.Match", "Match")
                        .WithMany()
                        .HasForeignKey("MatchId");

                    b.HasOne("Master.Matches.MatchInstance", "MatchInstance")
                        .WithMany()
                        .HasForeignKey("MatchInstanceId");

                    b.HasOne("Master.Majors.Major", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");
                });

            modelBuilder.Entity("Master.Matches.Match", b =>
                {
                    b.HasOne("Master.Authentication.User", "CreatorUser")
                        .WithMany()
                        .HasForeignKey("CreatorUserId");

                    b.HasOne("Master.Authentication.User", "DeleterUser")
                        .WithMany()
                        .HasForeignKey("DeleterUserId");

                    b.HasOne("Master.Authentication.User", "LastModifierUser")
                        .WithMany()
                        .HasForeignKey("LastModifierUserId");
                });

            modelBuilder.Entity("Master.Matches.MatchInstance", b =>
                {
                    b.HasOne("Master.Authentication.User", "CreatorUser")
                        .WithMany()
                        .HasForeignKey("CreatorUserId");

                    b.HasOne("Master.Authentication.User", "DeleterUser")
                        .WithMany()
                        .HasForeignKey("DeleterUserId");

                    b.HasOne("Master.Authentication.User", "LastModifierUser")
                        .WithMany()
                        .HasForeignKey("LastModifierUserId");

                    b.HasOne("Master.Matches.Match", "Match")
                        .WithMany("MatchInstances")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master.Matches.MatchResource", b =>
                {
                    b.HasOne("Master.Majors.Major", "Major")
                        .WithMany()
                        .HasForeignKey("MajorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master.Matches.Match", "Match")
                        .WithMany()
                        .HasForeignKey("MatchId");

                    b.HasOne("Master.Matches.MatchInstance", "MatchInstance")
                        .WithMany()
                        .HasForeignKey("MatchInstanceId");

                    b.HasOne("Master.Majors.Major", "SubMajor")
                        .WithMany()
                        .HasForeignKey("SubMajorId");
                });

            modelBuilder.Entity("Master.MultiTenancy.Tenant", b =>
                {
                    b.HasOne("Master.Authentication.User", "CreatorUser")
                        .WithMany()
                        .HasForeignKey("CreatorUserId");

                    b.HasOne("Master.Authentication.User", "DeleterUser")
                        .WithMany()
                        .HasForeignKey("DeleterUserId");

                    b.HasOne("Master.Authentication.User", "LastModifierUser")
                        .WithMany()
                        .HasForeignKey("LastModifierUserId");
                });

            modelBuilder.Entity("Master.Organizations.Organization", b =>
                {
                    b.HasOne("Master.Organizations.Organization", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");
                });

            modelBuilder.Entity("Master.Prizes.Prize", b =>
                {
                    b.HasOne("Master.Majors.Major", "Major")
                        .WithMany()
                        .HasForeignKey("MajorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master.Matches.Match", "Match")
                        .WithMany()
                        .HasForeignKey("MatchId");

                    b.HasOne("Master.Matches.MatchInstance", "MatchInstance")
                        .WithMany()
                        .HasForeignKey("MatchInstanceId");
                });

            modelBuilder.Entity("Master.Prizes.PrizeSubMajor", b =>
                {
                    b.HasOne("Master.Majors.Major", "Major")
                        .WithMany()
                        .HasForeignKey("MajorId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Master.Prizes.Prize", "Prize")
                        .WithMany("PrizeSubMajors")
                        .HasForeignKey("PrizeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master.Projects.Project", b =>
                {
                    b.HasOne("Master.Authentication.User", "CreatorUser")
                        .WithMany()
                        .HasForeignKey("CreatorUserId");

                    b.HasOne("Master.Organizations.Organization", "DesignOrganization")
                        .WithMany()
                        .HasForeignKey("DesignOrganizationId");

                    b.HasOne("Master.Matches.MatchInstance", "MatchInstance")
                        .WithMany()
                        .HasForeignKey("MatchInstanceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master.Prizes.Prize", "Prize")
                        .WithMany()
                        .HasForeignKey("PrizeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master.Prizes.PrizeSubMajor", "PrizeSubMajor")
                        .WithMany()
                        .HasForeignKey("PrizeSubMajorId");
                });

            modelBuilder.Entity("Master.Projects.ProjectMajorInfo", b =>
                {
                    b.HasOne("Master.Projects.Project", "Project")
                        .WithMany("ProjectMajorInfos")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master.Projects.ProjectTraceLog", b =>
                {
                    b.HasOne("Master.Authentication.User", "CreatorUser")
                        .WithMany()
                        .HasForeignKey("CreatorUserId");

                    b.HasOne("Master.Projects.Project", "Project")
                        .WithMany("ProjectTraceLogs")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master.Reviews.Review", b =>
                {
                    b.HasOne("Master.Majors.Major", "Major")
                        .WithMany()
                        .HasForeignKey("MajorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master.Matches.MatchInstance", "MatchInstance")
                        .WithMany()
                        .HasForeignKey("MatchInstanceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master.Reviews.ReviewRound", b =>
                {
                    b.HasOne("Master.Reviews.Review", "Review")
                        .WithMany("ReviewRounds")
                        .HasForeignKey("ReviewId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master.Authentication.RolePermissionSetting", b =>
                {
                    b.HasOne("Master.Authentication.Role")
                        .WithMany("Permissions")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master.Authentication.UserPermissionSetting", b =>
                {
                    b.HasOne("Master.Authentication.User")
                        .WithMany("Permissions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
