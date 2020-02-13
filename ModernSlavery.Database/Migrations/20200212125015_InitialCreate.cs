﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ConsoleApp1.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    FeedbackId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Difficulty = table.Column<int>(nullable: true),
                    NewsArticle = table.Column<bool>(nullable: true),
                    SocialMedia = table.Column<bool>(nullable: true),
                    CompanyIntranet = table.Column<bool>(nullable: true),
                    EmployerUnion = table.Column<bool>(nullable: true),
                    InternetSearch = table.Column<bool>(nullable: true),
                    Charity = table.Column<bool>(nullable: true),
                    LobbyGroup = table.Column<bool>(nullable: true),
                    Report = table.Column<bool>(nullable: true),
                    OtherSource = table.Column<bool>(nullable: true),
                    FindOutAboutGpg = table.Column<bool>(nullable: true),
                    ReportOrganisationGpgData = table.Column<bool>(nullable: true),
                    CloseOrganisationGpg = table.Column<bool>(nullable: true),
                    ViewSpecificOrganisationGpg = table.Column<bool>(nullable: true),
                    ActionsToCloseGpg = table.Column<bool>(nullable: true),
                    OtherReason = table.Column<bool>(nullable: true),
                    EmployeeInterestedInOrganisationData = table.Column<bool>(nullable: true),
                    ManagerInvolvedInGpgReport = table.Column<bool>(nullable: true),
                    ResponsibleForReportingGpg = table.Column<bool>(nullable: true),
                    PersonInterestedInGeneralGpg = table.Column<bool>(nullable: true),
                    PersonInterestedInSpecificOrganisationGpg = table.Column<bool>(nullable: true),
                    OtherPerson = table.Column<bool>(nullable: true),
                    Details = table.Column<string>(maxLength: 2000, nullable: true),
                    EmailAddress = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "(getdate())"),
                    OtherSourceText = table.Column<string>(maxLength: 2000, nullable: true),
                    OtherReasonText = table.Column<string>(maxLength: 2000, nullable: true),
                    OtherPersonText = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.FeedbackId);
                });

            migrationBuilder.CreateTable(
                name: "PublicSectorTypes",
                columns: table => new
                {
                    PublicSectorTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.PublicSectorTypes", x => x.PublicSectorTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ReminderEmails",
                columns: table => new
                {
                    ReminderEmailId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(nullable: false),
                    SectorType = table.Column<int>(nullable: false),
                    DateSent = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderEmails", x => x.ReminderEmailId);
                });

            migrationBuilder.CreateTable(
                name: "SicSections",
                columns: table => new
                {
                    SicSectionId = table.Column<string>(maxLength: 1, nullable: false),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.SicSections", x => x.SicSectionId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobTitle = table.Column<string>(maxLength: 50, nullable: false),
                    Firstname = table.Column<string>(maxLength: 50, nullable: false),
                    Lastname = table.Column<string>(maxLength: 50, nullable: false),
                    EmailAddress = table.Column<string>(maxLength: 255, nullable: false),
                    ContactJobTitle = table.Column<string>(maxLength: 50, nullable: true),
                    ContactFirstName = table.Column<string>(maxLength: 50, nullable: true),
                    ContactLastName = table.Column<string>(maxLength: 50, nullable: true),
                    ContactOrganisation = table.Column<string>(maxLength: 100, nullable: true),
                    ContactEmailAddress = table.Column<string>(maxLength: 255, nullable: true),
                    ContactPhoneNumber = table.Column<string>(maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(maxLength: 250, nullable: false),
                    EmailVerifyHash = table.Column<string>(maxLength: 250, nullable: true),
                    EmailVerifySendDate = table.Column<DateTime>(nullable: true),
                    EmailVerifiedDate = table.Column<DateTime>(nullable: true),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    LoginAttempts = table.Column<int>(nullable: false),
                    LoginDate = table.Column<DateTime>(nullable: true),
                    ResetSendDate = table.Column<DateTime>(nullable: true),
                    ResetAttempts = table.Column<int>(nullable: false),
                    VerifyAttemptDate = table.Column<DateTime>(nullable: true),
                    VerifyAttempts = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    HashingAlgorithm = table.Column<int>(nullable: false),
                    Salt = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "SicCodes",
                columns: table => new
                {
                    SicCodeId = table.Column<int>(nullable: false),
                    SicSectionId = table.Column<string>(maxLength: 1, nullable: false),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.SicCodes", x => x.SicCodeId);
                    table.ForeignKey(
                        name: "FK_dbo.SicCodes_dbo.SicSections_SicSectionId",
                        column: x => x.SicSectionId,
                        principalTable: "SicSections",
                        principalColumn: "SicSectionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    Key = table.Column<byte>(nullable: false),
                    Value = table.Column<string>(maxLength: 50, nullable: true),
                    Modified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.UserSettings", x => new { x.UserId, x.Key });
                    table.ForeignKey(
                        name: "FK_dbo.UserSettings_dbo.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStatus",
                columns: table => new
                {
                    UserStatusId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.UserStatus", x => x.UserStatusId);
                    table.ForeignKey(
                        name: "FK_dbo.UserStatus_dbo.Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.UserStatus_dbo.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AddressStatus",
                columns: table => new
                {
                    AddressStatusId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.AddressStatus", x => x.AddressStatusId);
                    table.ForeignKey(
                        name: "FK_dbo.AddressStatus_dbo.Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Organisations",
                columns: table => new
                {
                    OrganisationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyNumber = table.Column<string>(maxLength: 10, nullable: true),
                    OrganisationName = table.Column<string>(maxLength: 100, nullable: false),
                    SectorTypeId = table.Column<int>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    DUNSNumber = table.Column<string>(maxLength: 10, nullable: true),
                    EmployerReference = table.Column<string>(maxLength: 10, nullable: true),
                    DateOfCessation = table.Column<DateTime>(nullable: true),
                    LatestAddressId = table.Column<long>(nullable: true),
                    LatestReturnId = table.Column<long>(nullable: true),
                    LatestScopeId = table.Column<long>(nullable: true),
                    LatestRegistration_UserId = table.Column<long>(nullable: true),
                    LatestRegistration_OrganisationId = table.Column<long>(nullable: true),
                    SecurityCode = table.Column<string>(nullable: true),
                    SecurityCodeExpiryDateTime = table.Column<DateTime>(nullable: true),
                    SecurityCodeCreatedDateTime = table.Column<DateTime>(nullable: true),
                    LatestPublicSectorTypeId = table.Column<long>(nullable: true),
                    OptedOutFromCompaniesHouseUpdate = table.Column<bool>(nullable: false, defaultValueSql: "(CONVERT([bit],(0)))"),
                    LastCheckedAgainstCompaniesHouse = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Organisations", x => x.OrganisationId);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "(getdate())"),
                    OrganisationId = table.Column<long>(nullable: true),
                    OriginalUserId = table.Column<long>(nullable: true),
                    ImpersonatedUserId = table.Column<long>(nullable: true),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ImpersonatedUserId",
                        column: x => x.ImpersonatedUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_OriginalUserId",
                        column: x => x.OriginalUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationAddresses",
                columns: table => new
                {
                    AddressId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<long>(nullable: false),
                    Address1 = table.Column<string>(maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(maxLength: 100, nullable: true),
                    Address3 = table.Column<string>(maxLength: 100, nullable: true),
                    TownCity = table.Column<string>(maxLength: 100, nullable: true),
                    County = table.Column<string>(maxLength: 100, nullable: true),
                    Country = table.Column<string>(maxLength: 100, nullable: true),
                    PoBox = table.Column<string>(maxLength: 30, nullable: true),
                    PostCode = table.Column<string>(maxLength: 20, nullable: true),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    IsUkAddress = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationAddresses", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationAddresses_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationNames",
                columns: table => new
                {
                    OrganisationNameId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationNames", x => x.OrganisationNameId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationNames_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationPublicSectorTypes",
                columns: table => new
                {
                    OrganisationPublicSectorTypeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicSectorTypeId = table.Column<int>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Retired = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationPublicSectorTypes", x => x.OrganisationPublicSectorTypeId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationPublicSectorTypes_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationPublicSectorTypes_dbo.PublicSectorTypes_PublicSectorTypeId",
                        column: x => x.PublicSectorTypeId,
                        principalTable: "PublicSectorTypes",
                        principalColumn: "PublicSectorTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationReferences",
                columns: table => new
                {
                    OrganisationReferenceId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    ReferenceName = table.Column<string>(maxLength: 100, nullable: false),
                    ReferenceValue = table.Column<string>(maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationReferences", x => x.OrganisationReferenceId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationReferences_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationScopes",
                columns: table => new
                {
                    OrganisationScopeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    ScopeStatusId = table.Column<int>(nullable: false),
                    ScopeStatusDate = table.Column<DateTime>(nullable: false),
                    RegisterStatusId = table.Column<int>(nullable: false),
                    RegisterStatusDate = table.Column<DateTime>(nullable: false),
                    ContactFirstname = table.Column<string>(maxLength: 50, nullable: true),
                    ContactLastname = table.Column<string>(maxLength: 50, nullable: true),
                    ContactEmailAddress = table.Column<string>(maxLength: 255, nullable: true),
                    ReadGuidance = table.Column<bool>(nullable: true),
                    Reason = table.Column<string>(maxLength: 1000, nullable: true),
                    CampaignId = table.Column<string>(maxLength: 50, nullable: true),
                    SnapshotDate = table.Column<DateTime>(nullable: false, defaultValueSql: "('1900-01-01T00:00:00.000')"),
                    StatusId = table.Column<byte>(nullable: false, defaultValueSql: "((0))"),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationScopes", x => x.OrganisationScopeId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationScopes_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationSicCodes",
                columns: table => new
                {
                    OrganisationSicCodeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SicCodeId = table.Column<int>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Retired = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationSicCodes", x => x.OrganisationSicCodeId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationSicCodes_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationSicCodes_dbo.SicCodes_SicCodeId",
                        column: x => x.SicCodeId,
                        principalTable: "SicCodes",
                        principalColumn: "SicCodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationStatus",
                columns: table => new
                {
                    OrganisationStatusId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationStatus", x => x.OrganisationStatusId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationStatus_dbo.Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationStatus_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Returns",
                columns: table => new
                {
                    ReturnId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    AccountingDate = table.Column<DateTime>(nullable: false),
                    DiffMeanHourlyPayPercent = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DiffMedianHourlyPercent = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DiffMeanBonusPercent = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    DiffMedianBonusPercent = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    MaleMedianBonusPayPercent = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    FemaleMedianBonusPayPercent = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    MaleLowerPayBand = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    FemaleLowerPayBand = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    MaleMiddlePayBand = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    FemaleMiddlePayBand = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    MaleUpperPayBand = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    FemaleUpperPayBand = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    MaleUpperQuartilePayBand = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    FemaleUpperQuartilePayBand = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    CompanyLinkToGPGInfo = table.Column<string>(maxLength: 255, nullable: true),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    JobTitle = table.Column<string>(maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(maxLength: 50, nullable: true),
                    LastName = table.Column<string>(maxLength: 50, nullable: true),
                    MinEmployees = table.Column<int>(nullable: false, defaultValueSql: "((0))"),
                    MaxEmployees = table.Column<int>(nullable: false, defaultValueSql: "((0))"),
                    LateReason = table.Column<string>(maxLength: 200, nullable: true),
                    Modifications = table.Column<string>(maxLength: 200, nullable: true),
                    EHRCResponse = table.Column<bool>(nullable: false, defaultValueSql: "((0))"),
                    IsLateSubmission = table.Column<bool>(nullable: false, defaultValueSql: "(CONVERT([bit],(0)))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Returns", x => x.ReturnId);
                    table.ForeignKey(
                        name: "FK_dbo.Returns_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOrganisations",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    PINHash = table.Column<string>(maxLength: 250, nullable: true),
                    PINSentDate = table.Column<DateTime>(nullable: true),
                    PINConfirmedDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttemptDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttempts = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    AddressId = table.Column<long>(nullable: true),
                    MethodId = table.Column<int>(nullable: false, defaultValueSql: "((0))"),
                    PIN = table.Column<string>(nullable: true),
                    PITPNotifyLetterId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.UserOrganisations", x => new { x.UserId, x.OrganisationId });
                    table.ForeignKey(
                        name: "FK_dbo.UserOrganisations_dbo.OrganisationAddresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "OrganisationAddresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.UserOrganisations_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.UserOrganisations_dbo.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReturnStatus",
                columns: table => new
                {
                    ReturnStatusId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ReturnStatus", x => x.ReturnStatusId);
                    table.ForeignKey(
                        name: "FK_dbo.ReturnStatus_dbo.Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.ReturnStatus_dbo.Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressId",
                table: "AddressStatus",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ByUserId",
                table: "AddressStatus",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusDate",
                table: "AddressStatus",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ImpersonatedUserId",
                table: "AuditLogs",
                column: "ImpersonatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OrganisationId",
                table: "AuditLogs",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OriginalUserId",
                table: "AuditLogs",
                column: "OriginalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "OrganisationAddresses",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusDate",
                table: "OrganisationAddresses",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_StatusId",
                table: "OrganisationAddresses",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Created",
                table: "OrganisationNames",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "OrganisationNames",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "OrganisationNames",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_Created",
                table: "OrganisationPublicSectorTypes",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "OrganisationPublicSectorTypes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicSectorTypeId",
                table: "OrganisationPublicSectorTypes",
                column: "PublicSectorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Retired",
                table: "OrganisationPublicSectorTypes",
                column: "Retired");

            migrationBuilder.CreateIndex(
                name: "IX_Created",
                table: "OrganisationReferences",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "OrganisationReferences",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceName",
                table: "OrganisationReferences",
                column: "ReferenceName");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceValue",
                table: "OrganisationReferences",
                column: "ReferenceValue");

            migrationBuilder.CreateIndex(
                name: "idx_Organisations_CompanyNumber",
                table: "Organisations",
                column: "CompanyNumber",
                unique: true,
                filter: "([CompanyNumber] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "idx_Organisations_DUNSNumber",
                table: "Organisations",
                column: "DUNSNumber",
                unique: true,
                filter: "([DUNSNumber] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "idx_Organisations_EmployerReference",
                table: "Organisations",
                column: "EmployerReference",
                unique: true,
                filter: "([EmployerReference] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_LatestAddressId",
                table: "Organisations",
                column: "LatestAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_LatestPublicSectorTypeId",
                table: "Organisations",
                column: "LatestPublicSectorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LatestReturnId",
                table: "Organisations",
                column: "LatestReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_LatestScopeId",
                table: "Organisations",
                column: "LatestScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationName",
                table: "Organisations",
                column: "OrganisationName");

            migrationBuilder.CreateIndex(
                name: "IX_SectorTypeId",
                table: "Organisations",
                column: "SectorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusId",
                table: "Organisations",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                table: "Organisations",
                columns: new[] { "LatestRegistration_UserId", "LatestRegistration_OrganisationId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "OrganisationScopes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterStatusId",
                table: "OrganisationScopes",
                column: "RegisterStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ScopeStatusDate",
                table: "OrganisationScopes",
                column: "ScopeStatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScopeStatusId",
                table: "OrganisationScopes",
                column: "ScopeStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotDate",
                table: "OrganisationScopes",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_StatusId",
                table: "OrganisationScopes",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Created",
                table: "OrganisationSicCodes",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "OrganisationSicCodes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_Retired",
                table: "OrganisationSicCodes",
                column: "Retired");

            migrationBuilder.CreateIndex(
                name: "IX_SicCodeId",
                table: "OrganisationSicCodes",
                column: "SicCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ByUserId",
                table: "OrganisationStatus",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "OrganisationStatus",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusDate",
                table: "OrganisationStatus",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDate",
                table: "Returns",
                column: "AccountingDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "Returns",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusId",
                table: "Returns",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ByUserId",
                table: "ReturnStatus",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnId",
                table: "ReturnStatus",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusDate",
                table: "ReturnStatus",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_SicSectionId",
                table: "SicCodes",
                column: "SicSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressId",
                table: "UserOrganisations",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "UserOrganisations",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserId",
                table: "UserOrganisations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactEmailAddress",
                table: "Users",
                column: "ContactEmailAddress");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPhoneNumber",
                table: "Users",
                column: "ContactPhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmailAddress",
                table: "Users",
                column: "EmailAddress");

            migrationBuilder.CreateIndex(
                name: "IX_StatusId",
                table: "Users",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserId",
                table: "UserSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ByUserId",
                table: "UserStatus",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusDate",
                table: "UserStatus",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserId",
                table: "UserStatus",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.AddressStatus_dbo.OrganisationAddresses_AddressId",
                table: "AddressStatus",
                column: "AddressId",
                principalTable: "OrganisationAddresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationAddresses_LatestAddressId",
                table: "Organisations",
                column: "LatestAddressId",
                principalTable: "OrganisationAddresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationPublicSectorTypes_LatestPublicSectorTypeId",
                table: "Organisations",
                column: "LatestPublicSectorTypeId",
                principalTable: "OrganisationPublicSectorTypes",
                principalColumn: "OrganisationPublicSectorTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.Returns_LatestReturnId",
                table: "Organisations",
                column: "LatestReturnId",
                principalTable: "Returns",
                principalColumn: "ReturnId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationScopes_LatestScopeId",
                table: "Organisations",
                column: "LatestScopeId",
                principalTable: "OrganisationScopes",
                principalColumn: "OrganisationScopeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.UserOrganisations_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                table: "Organisations",
                columns: new[] { "LatestRegistration_UserId", "LatestRegistration_OrganisationId" },
                principalTable: "UserOrganisations",
                principalColumns: new[] { "UserId", "OrganisationId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("CREATE USER [ReportsReaderDv] FOR LOGIN [ReportsReaderLogin];");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::[dbo].[OrganisationNames] TO [ReportsReaderDv] AS [dbo];");

            migrationBuilder.Sql("CREATE FUNCTION [dbo].[OrganisationSectorTypeIdToString](@organisationSectorTypeId tinyint) RETURNS varchar(20) AS BEGIN DECLARE @Result varchar(20) SELECT @Result = CASE @organisationSectorTypeId WHEN 0 THEN 'Unknown' WHEN 1 THEN 'Private' WHEN 2 THEN 'Public' ELSE 'Error' END + ' (' + CAST(@organisationSectorTypeId AS VARCHAR(3)) + ')' RETURN @Result END");
            migrationBuilder.Sql("GRANT EXECUTE ON OBJECT::[dbo].[OrganisationSectorTypeIdToString] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE FUNCTION [dbo].[OrganisationStatusIdToString](@organisationStatusId tinyint) RETURNS varchar(20) AS BEGIN DECLARE @Result varchar(20) SELECT @Result = CASE @organisationStatusId WHEN 0 THEN 'Unknown' WHEN 1 THEN 'New' WHEN 2 THEN 'Suspended' WHEN 3 THEN 'Active' WHEN 4 THEN 'Retired' WHEN 5 THEN 'Pending' WHEN 6 THEN 'Deleted' ELSE 'Error' END + ' (' + CAST(@organisationStatusId AS VARCHAR(3)) + ')' RETURN @Result END");
            migrationBuilder.Sql("GRANT EXECUTE ON OBJECT::[dbo].[OrganisationStatusIdToString] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE VIEW [dbo].[OrganisationAddressInfoView] AS SELECT organisationAdd.OrganisationId, CASE organisationAdd.[StatusId] WHEN 0 THEN 'Unknown' WHEN 1 THEN 'New' WHEN 2 THEN 'Suspended' WHEN 3 THEN 'Active' WHEN 5 THEN 'Pending' WHEN 6 THEN 'Retired' ELSE 'Error' END + ' (' + CAST(organisationAdd.StatusId AS VARCHAR(2)) + ')' AS AddressStatus, REPLACE(COALESCE(RTRIM(LTRIM(organisationAdd.Address1)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.Address2)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.Address3)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.TownCity)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.County)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.PoBox)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.PostCode)), '') + COALESCE(' (' + RTRIM(LTRIM(organisationAdd.Country)) + ')', ''), ' ,', ',') AS FullAddress, organisationAdd.StatusDetails AS AddressStatusDetails, organisationAdd.StatusDate AS AddressStatusDate, organisationAdd.Source AS AddressSource, organisationAdd.Created AS AddressCreated, organisationAdd.Modified AS AddressModified FROM(SELECT AddressId, CreatedByUserId, CASE WHEN(LEN([Address1]) > 0) THEN[Address1] ELSE NULL END AS Address1, CASE WHEN(LEN([Address2]) > 0) THEN[Address2] ELSE NULL END AS Address2 ,CASE WHEN(LEN([Address3]) > 0) THEN[Address3] ELSE NULL END AS Address3 ,CASE WHEN(LEN([TownCity]) > 0) THEN[TownCity] ELSE NULL END AS TownCity ,CASE WHEN(LEN([County]) > 0) THEN[County] ELSE NULL END AS County ,CASE WHEN(LEN([Country]) > 0) THEN[Country] ELSE NULL END AS Country ,CASE WHEN(LEN([PoBox]) > 0) THEN[PoBox] ELSE NULL END AS PoBox ,CASE WHEN(LEN([PostCode]) > 0) THEN[PostCode] ELSE NULL END AS PostCode ,StatusId ,StatusDate ,StatusDetails ,Created ,Modified ,OrganisationId ,Source FROM OrganisationAddresses ) AS organisationAdd INNER JOIN Organisations AS orgs ON orgs.OrganisationId = organisationAdd.OrganisationId");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::[dbo].[OrganisationAddressInfoView] TO [ReportsReaderDv] AS [dbo];");

            migrationBuilder.Sql("CREATE VIEW [dbo].[OrganisationInfoView] AS SELECT orgs.OrganisationId, orgs.EmployerReference, orgs.DUNSNumber, orgs.CompanyNumber, orgs.OrganisationName, [dbo].OrganisationSectorTypeIdToString(orgs.[SectorTypeId]) AS SectorType, dbo.OrganisationStatusIdToString(orgs.StatusId) AS OrganisationStatus, orgs.[SecurityCode], orgs.[SecurityCodeExpiryDateTime], orgs.[SecurityCodeCreatedDateTime] FROM Organisations AS orgs");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::dbo.[OrganisationInfoView] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE VIEW [dbo].[OrganisationRegistrationInfoView] AS SELECT uo.OrganisationId, u.Firstname + ' ' + u.Lastname + ' [' + u.JobTitle + ']' AS UserInfo, u.ContactFirstName + ' ' + u.ContactLastName + ' [' + u.ContactJobTitle + ']' AS ContactInfo, CASE uo.[MethodId] WHEN 0 THEN 'Unknown' WHEN 1 THEN 'PinInPost' WHEN 2 THEN 'EmailDomain' WHEN 3 THEN 'Manual' WHEN 4 THEN 'Fasttrack' ELSE 'Error' END + ' (' + CAST(uo.MethodId AS VARCHAR(2)) + ')' AS RegistrationMethod, uo.PINSentDate, uo.PINConfirmedDate, uo.ConfirmAttemptDate, uo.ConfirmAttempts FROM UserOrganisations AS uo INNER JOIN Users AS u ON u.UserId = uo.UserId");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::dbo.[OrganisationRegistrationInfoView] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE VIEW [dbo].[OrganisationSicCodeInfoView] AS SELECT osc.OrganisationId, sc.[SicCodeId], rank() OVER(PARTITION BY osc.OrganisationId ORDER BY sc.[SicCodeId], osc.[Source] ASC) AS SicCodeRankWithinOrganisation, CASE WHEN(CHARINDEX('@', osc.[Source]) > 0) THEN 'User' ELSE osc.[Source] END AS[Source], sc.[Description] AS CodeDescription, ss.SicSectionId, ss.[Description] AS SectionDescription FROM[SicSections] AS ss RIGHT JOIN[SicCodes] AS sc ON sc.SicSectionId = ss.SicSectionId RIGHT JOIN(SELECT sicCodeId, organisationid,[source] FROM [OrganisationSicCodes] GROUP BY sicCodeId, organisationid,[source]) AS osc ON osc.SicCodeId = sc.SicCodeId");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::dbo.[OrganisationSicCodeInfoView] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE VIEW [dbo].[OrganisationScopeAndReturnInfoView] AS SELECT orgs.OrganisationId, orgs.OrganisationName, orgs.EmployerReference, orgs.CompanyNumber, dbo.OrganisationStatusIdToString(orgs.StatusId) AS OrganisationStatus, dbo.OrganisationSectorTypeIdToString(orgs.SectorTypeId) AS SectorType, CASE OrgScopes.[ScopeStatusId] WHEN 0 THEN 'Unknown' WHEN 1 THEN 'In scope' WHEN 2 THEN 'Out of scope' WHEN 3 THEN 'Presumed in scope' WHEN 4 THEN 'Presumed out of scope' ELSE 'Error' END + ' (' + CAST(OrgScopes.ScopeStatusId AS VARCHAR(3)) + ')' AS ScopeStatus, YEAR(OrgScopes.SnapshotDate) AS SnapshotDate, OrgSicCodeView.SectionDescription AS SicCodeSectionDescription, retrns.ReturnId, CASE WHEN retrns.MinEmployees = 0 AND retrns.MaxEmployees = 0 THEN 'Not Provided' WHEN retrns.MinEmployees = 0 AND retrns.MaxEmployees = 249 THEN 'Less than 250' WHEN(retrns.MinEmployees = 250 AND retrns.MaxEmployees = 499) OR(retrns.MinEmployees = 500 AND retrns.MaxEmployees = 999) OR (retrns.MinEmployees = 1000 AND retrns.MaxEmployees = 4999) THEN CAST(retrns.MinEmployees AS VARCHAR) + ' to ' + CAST(retrns.MaxEmployees AS VARCHAR) WHEN retrns.MinEmployees = 5000 AND retrns.MaxEmployees = 19999 THEN CAST(retrns.MinEmployees AS VARCHAR) + ' to 19,999' WHEN retrns.MinEmployees = 20000 AND retrns.MaxEmployees = 2147483647 THEN '20,000 or more' ELSE 'Error min(' + CAST(retrns.MinEmployees AS VARCHAR) + '), max (' + CAST(retrns.MaxEmployees AS VARCHAR) + ')' END AS OrganisationSize, OrgPubSectType.PublicSectorDescription FROM Organisations AS orgs INNER JOIN OrganisationScopes AS OrgScopes ON OrgScopes.OrganisationId = orgs.OrganisationId AND OrgScopes.StatusId = 3 LEFT OUTER JOIN OrganisationSicCodeInfoView AS OrgSicCodeView ON OrgSicCodeView.OrganisationId = orgs.OrganisationId AND OrgSicCodeView.SicCodeRankWithinOrganisation = 1 LEFT OUTER JOIN [Returns] AS retrns ON retrns.OrganisationId = OrgScopes.OrganisationId AND retrns.AccountingDate = OrgScopes.SnapshotDate AND retrns.StatusId = 3 LEFT OUTER JOIN(SELECT opst.OrganisationId, pst.Description AS PublicSectorDescription FROM OrganisationPublicSectorTypes AS opst INNER JOIN PublicSectorTypes AS pst ON pst.PublicSectorTypeId = opst.PublicSectorTypeId AND opst.Retired IS NULL) AS OrgPubSectType ON OrgPubSectType.OrganisationId = orgs.OrganisationId");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::dbo.[OrganisationScopeAndReturnInfoView] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE VIEW [dbo].[OrganisationScopeInfoView] AS SELECT OrganisationId,CASE scopes.scopestatusid WHEN 1 THEN 'In Scope' WHEN 2 THEN 'Out of Scope' WHEN 3 THEN 'Presumed in Scope' WHEN 4 THEN 'Presumed out of Scope' ELSE 'Unknown' END + ' (' + CAST(ScopeStatusId AS VARCHAR(2)) + ')' AS ScopeStatus,ScopeStatusDate,CASE scopes.RegisterStatusid WHEN 0 THEN 'Unknown' WHEN 1 THEN 'RegisterSkipped' WHEN 2 THEN 'RegisterPending' WHEN 3 THEN 'RegisterComplete' WHEN 4 THEN 'RegisterCancelled' ELSE 'Error' END + ' (' + CAST(RegisterStatusId AS VARCHAR(2)) + ')' AS RegisterStatus,RegisterStatusDate,YEAR(SnapshotDate) AS snapshotYear FROM dbo.OrganisationScopes AS scopes");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::dbo.[OrganisationScopeInfoView] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE VIEW [dbo].[OrganisationSearchInfoView] AS SELECT orgs.OrganisationId, orgs.EmployerReference, orgs.DUNSNumber, orgs.CompanyNumber, orgs.OrganisationName, CASE orgs.[SectorTypeId] WHEN 0 THEN 'Unknown' WHEN 1 THEN 'Private' WHEN 2 THEN 'Public' ELSE 'Error' END + ' (' + CAST(orgs.SectorTypeId AS VARCHAR(2)) + ')' AS SectorType, CASE orgs.StatusId WHEN 0 THEN 'Unknown' WHEN 1 THEN 'New' WHEN 2 THEN 'Suspended' WHEN 3 THEN 'Active' WHEN 4 THEN 'Retired' WHEN 5 THEN 'Pending' ELSE 'Error' END + ' (' + CAST(orgs.StatusId AS VARCHAR(2)) + ')' AS OrganisationStatus FROM Organisations AS orgs");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::dbo.[OrganisationSearchInfoView] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE VIEW [dbo].[OrganisationSubmissionInfoView] AS SELECT ret.OrganisationId, CONVERT(DATE, ret.AccountingDate) AS LatestReturnAccountingDate, DATEADD(second, -1, DATEADD(year, 1, ret.AccountingDate)) AS ReportingDeadline, CASE ret.[StatusId] WHEN 0 THEN 'Unknown' WHEN 1 THEN 'Draft' WHEN 2 THEN 'Suspended' WHEN 3 THEN 'Submitted' WHEN 4 THEN 'Retired' WHEN 5 THEN 'Deleted' ELSE 'Error' END + ' (' + CAST(ret.StatusId AS VARCHAR(2)) + ')' AS latestReturnStatus, ret.StatusDate AS latestReturnStatusDate, firstDates.dateFirstReportedInYear, ret.StatusDetails AS LatestReturnStatusDetails, CASE WHEN([Modified] > dateadd(second, -1, dateadd(year, 1, ret.accountingdate))) THEN 'true' ELSE 'false' END AS ReportedLate ,ret.LateReason AS LatestReturnLateReason ,CASE retstat.[StatusId] WHEN 0 			THEN 'Unknown' 		WHEN 1 			THEN 'Draft' 		WHEN 2 			THEN 'Suspended' 		WHEN 3 			THEN 'Submitted' 		WHEN 4 			THEN 'Retired' 		WHEN 5 			THEN 'Deleted' 		ELSE 'Error' 		END + ' (' + CAST(retstat.StatusId AS VARCHAR(2)) + ')' AS StatusId, retstat.StatusDate 	,retstat.StatusDetails 	,ret.Modifications AS ReturnModifiedFields 	,CASE ret.[EHRCResponse] WHEN 1 			THEN 'true' 		ELSE 'false' 		END AS EHRCResponse , ret.FirstName + ' ' + ret.LastName + ' [' + ret.JobTitle + ']' AS SubmittedBy, CASE WHEN ( [MinEmployees] = 0 AND[MaxEmployees] = 0) THEN 'Not provided' 		WHEN([MinEmployees] = 0 AND[MaxEmployees] = 249) THEN 'Employees 0 to 249' 		WHEN([MinEmployees] = 250 AND[MaxEmployees] = 499) THEN 'Employees 250 to 499' 		WHEN([MinEmployees] = 500 AND[MaxEmployees] = 999) THEN 'Employees 500 to 999' 		WHEN([MinEmployees] = 1000 AND[MaxEmployees] = 4999) THEN 'Employees 1,000 to 4,999' 		WHEN([MinEmployees] = 5000 AND[MaxEmployees] = 19999) THEN 'Employees 5,000 to 19,999' 		WHEN([MinEmployees] = 20000 AND[MaxEmployees] = 2147483647) THEN 'Employees 20,000 or more' 		ELSE 'Error' 		END AS OrganisationSize 	,ret.DiffMeanHourlyPayPercent 	,ret.DiffMedianHourlyPercent 	,ret.DiffMeanBonusPercent 	,ret.DiffMedianBonusPercent 	,ret.MaleMedianBonusPayPercent 	,ret.FemaleMedianBonusPayPercent 	,ret.MaleLowerPayBand 	,ret.FemaleLowerPayBand 	,ret.MaleMiddlePayBand 	,ret.FemaleMiddlePayBand 	,ret.MaleUpperPayBand 	,ret.FemaleUpperPayBand 	,ret.MaleUpperQuartilePayBand 	,ret.FemaleUpperQuartilePayBand 	,ret.CompanyLinkToGPGInfo FROM [Returns] AS ret LEFT OUTER JOIN ReturnStatus AS retstat ON retstat.ReturnId = ret.ReturnId LEFT OUTER JOIN(SELECT[OrganisationId] , [AccountingDate] , min([Modified]) AS dateFirstReportedInYear FROM [Returns] AS ret JOIN[ReturnStatus] AS retst ON retst.ReturnId = ret.ReturnId GROUP BY [OrganisationId] , [AccountingDate] ) AS firstDates ON firstDates.OrganisationId = ret.OrganisationId AND firstDates.AccountingDate = ret.AccountingDate");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::dbo.[OrganisationSubmissionInfoView] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE VIEW [dbo].[UserInfoView] AS SELECT UserId, CASE[StatusId] WHEN 0 THEN 'Unknown' WHEN 1 THEN 'New' WHEN 2 THEN 'Suspended' WHEN 3 THEN 'Active' WHEN 4 THEN 'Retired' ELSE 'Error' END + ' (' + CAST(StatusId AS VARCHAR(2)) + ')' AS StatusId, StatusDate, StatusDetails, Firstname, Lastname, JobTitle, ContactFirstName, ContactLastName, ContactJobTitle, ContactOrganisation, ContactPhoneNumber, EmailVerifySendDate, EmailVerifiedDate, LoginAttempts, LoginDate, ResetSendDate, ResetAttempts, VerifyAttemptDate, VerifyAttempts FROM Users");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::dbo.[UserInfoView] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE VIEW [dbo].[UserLinkedOrganisationsView] AS SELECT usrOrgs.UserId, orgs.OrganisationId, orgs.DUNSNumber, orgs.EmployerReference, orgs.CompanyNumber, orgs.OrganisationName, dbo.OrganisationSectorTypeIdToString(orgs.SectorTypeId) AS SectorTypeId, dbo.OrganisationStatusIdToString(orgs.StatusId) AS StatusId FROM UserOrganisations AS usrOrgs INNER JOIN Organisations AS orgs ON orgs.OrganisationId = usrOrgs.OrganisationId");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::dbo.[UserLinkedOrganisationsView] TO [ReportsReaderDv]; ");

            migrationBuilder.Sql("CREATE VIEW [dbo].[UserStatusInfoView] AS SELECT ust.ByUserId AS UserId, modifyingUser.Firstname + ' ' + modifyingUser.Lastname + ' [' + modifyingUser.JobTitle + ']' AS UserName, CASE usr.[StatusId] WHEN 0 THEN 'Unknown' WHEN 1 THEN 'New' WHEN 2 THEN 'Suspended' WHEN 3 THEN 'Active' WHEN 4 THEN 'Retired' ELSE 'Error' END + ' (' + CAST(usr.StatusId AS VARCHAR(2)) + ')' AS StatusId, ust.StatusDate, ust.StatusDetails, usr.Firstname + ' ' + usr.Lastname + ' [' + usr.JobTitle + ']' AS StatusChangedBy FROM UserStatus AS ust INNER JOIN Users AS usr ON usr.UserId = ust.UserId INNER JOIN Users AS modifyingUser ON modifyingUser.UserId = ust.ByUserId");
            migrationBuilder.Sql("GRANT SELECT ON OBJECT::dbo.[UserStatusInfoView] TO [ReportsReaderDv]; ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationAddresses_LatestAddressId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.UserOrganisations_dbo.OrganisationAddresses_AddressId",
                table: "UserOrganisations");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.UserOrganisations_dbo.Users_UserId",
                table: "UserOrganisations");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.OrganisationPublicSectorTypes_dbo.Organisations_OrganisationId",
                table: "OrganisationPublicSectorTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.OrganisationScopes_dbo.Organisations_OrganisationId",
                table: "OrganisationScopes");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Returns_dbo.Organisations_OrganisationId",
                table: "Returns");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.UserOrganisations_dbo.Organisations_OrganisationId",
                table: "UserOrganisations");

            migrationBuilder.DropTable(
                name: "AddressStatus");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "OrganisationNames");

            migrationBuilder.DropTable(
                name: "OrganisationReferences");

            migrationBuilder.DropTable(
                name: "OrganisationSicCodes");

            migrationBuilder.DropTable(
                name: "OrganisationStatus");

            migrationBuilder.DropTable(
                name: "ReminderEmails");

            migrationBuilder.DropTable(
                name: "ReturnStatus");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "UserStatus");

            migrationBuilder.DropTable(
                name: "SicCodes");

            migrationBuilder.DropTable(
                name: "SicSections");

            migrationBuilder.DropTable(
                name: "OrganisationAddresses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Organisations");

            migrationBuilder.DropTable(
                name: "OrganisationPublicSectorTypes");

            migrationBuilder.DropTable(
                name: "Returns");

            migrationBuilder.DropTable(
                name: "OrganisationScopes");

            migrationBuilder.DropTable(
                name: "UserOrganisations");

            migrationBuilder.DropTable(
                name: "PublicSectorTypes");

            migrationBuilder.Sql("DROP VIEW[OrganisationAddressInfoView]");
            migrationBuilder.Sql("DROP VIEW[OrganisationInfoView]");
            migrationBuilder.Sql("DROP VIEW[OrganisationRegistrationInfoView]");
            migrationBuilder.Sql("DROP VIEW[OrganisationScopeAndReturnInfoView]");
            migrationBuilder.Sql("DROP VIEW[OrganisationSearchInfoView]");
            migrationBuilder.Sql("DROP VIEW[OrganisationSicCodeInfoView]");
            migrationBuilder.Sql("DROP VIEW[OrganisationSubmissionInfoView]");
            migrationBuilder.Sql("DROP VIEW[UserInfoView]");
            migrationBuilder.Sql("DROP VIEW[UserLinkedOrganisationsView]");
            migrationBuilder.Sql("DROP VIEW[UserStatusInfoView]");

            migrationBuilder.Sql("DROP FUNCTION [OrganisationSectorTypeIdToString]");
            migrationBuilder.Sql("DROP FUNCTION [OrganisationStatusIdToString]");

            migrationBuilder.Sql("DROP USER [ReportsReaderDv]");
            migrationBuilder.Sql("DROP LOGIN [ReportsReaderLogin];");
        }
    }
}