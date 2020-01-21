using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JinCreek.Server.Common.Migrations
{
    public partial class mdbOne : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lte",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LteName = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    LteAdapter = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    SoftwareRadioState = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(type: "VARCHAR(255) BINARY", nullable: false),
                    Name = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    Address = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    DelegatePhone = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    AdminPhone = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    AdminMail = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    StartDay = table.Column<DateTime>(nullable: false),
                    EndDay = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    IsValid = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                    table.UniqueConstraint("Organization_Code_UQ", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "DeviceGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Version = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    OsType = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceGroup_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Domain",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DomainName = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domain", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Domain_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SimGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SimGroupName = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    PrimaryDns = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    SecondDns = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    Apn = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    NasAddress = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    Nw1AddressPool = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    Nw1AddressRange = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    ServerAddress = table.Column<string>(type: "LONGTEXT BINARY", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimGroup_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DeviceName = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    DeviceImei = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    ManagerNumber = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    Type = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    DeviceGroupId = table.Column<Guid>(nullable: false),
                    LteId = table.Column<Guid>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    DomainId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Device_Domain_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Device_DeviceGroup_DeviceGroupId",
                        column: x => x.DeviceGroupId,
                        principalTable: "DeviceGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Device_Lte_LteId",
                        column: x => x.LteId,
                        principalTable: "Lte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserGroupName = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    DomainId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroup_Domain_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sim",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Msisdn = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    Imsi = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    IccId = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    UserName = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    Password = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    SimGroupId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sim_SimGroup_SimGroupId",
                        column: x => x.SimGroupId,
                        principalTable: "SimGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdDeviceSettingOfflineWindowsSignIn",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    WindowsSignInListCacheDays = table.Column<int>(nullable: false),
                    AdDeviceId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdDeviceSettingOfflineWindowsSignIn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdDeviceSettingOfflineWindowsSignIn_Device_AdDeviceId",
                        column: x => x.AdDeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LastName = table.Column<string>(type: "LONGTEXT BINARY", nullable: true),
                    FirstName = table.Column<string>(type: "LONGTEXT BINARY", nullable: true),
                    AccountName = table.Column<string>(nullable: false),
                    UserType = table.Column<string>(nullable: false),
                    DomainId = table.Column<Guid>(nullable: true),
                    UserGroupId = table.Column<Guid>(nullable: true),
                    IsDisconnectWhenScreenLock = table.Column<bool>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    SuperAdminUser_Password = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.UniqueConstraint("User_AccountName_UQ", x => x.AccountName);
                    table.ForeignKey(
                        name: "FK_User_Domain_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_UserGroup_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SimDevice",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SimId = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<Guid>(nullable: false),
                    Nw2AddressPool = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    StartDay = table.Column<DateTime>(nullable: false),
                    EndDay = table.Column<DateTime>(nullable: false),
                    AuthPeriod = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimDevice_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SimDevice_Sim_SimId",
                        column: x => x.SimId,
                        principalTable: "Sim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactorCombination",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SimDeviceId = table.Column<Guid>(nullable: false),
                    EndUserId = table.Column<Guid>(nullable: false),
                    NwAddress = table.Column<string>(type: "LONGTEXT BINARY", nullable: false),
                    StartDay = table.Column<DateTime>(nullable: false),
                    EndDay = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactorCombination", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactorCombination_User_EndUserId",
                        column: x => x.EndUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FactorCombination_SimDevice_SimDeviceId",
                        column: x => x.SimDeviceId,
                        principalTable: "SimDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthenticationLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ConnectionTime = table.Column<DateTime>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    FactorCombinationId = table.Column<Guid>(nullable: true),
                    SimDeviceId = table.Column<Guid>(nullable: true),
                    MultiFactorAuthenticationLogSuccess_FactorCombinationId = table.Column<Guid>(nullable: true),
                    SimId = table.Column<Guid>(nullable: true),
                    SimDeviceAuthenticationLogSuccess_SimDeviceId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthenticationLog_FactorCombination_FactorCombinationId",
                        column: x => x.FactorCombinationId,
                        principalTable: "FactorCombination",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthenticationLog_SimDevice_SimDeviceId",
                        column: x => x.SimDeviceId,
                        principalTable: "SimDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthenticationLog_FactorCombination_MultiFactorAuthenticatio~",
                        column: x => x.MultiFactorAuthenticationLogSuccess_FactorCombinationId,
                        principalTable: "FactorCombination",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthenticationLog_Sim_SimId",
                        column: x => x.SimId,
                        principalTable: "Sim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthenticationLog_SimDevice_SimDeviceAuthenticationLogSucces~",
                        column: x => x.SimDeviceAuthenticationLogSuccess_SimDeviceId,
                        principalTable: "SimDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthenticationState",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TimeLimit = table.Column<DateTime>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    FactorCombinationId = table.Column<Guid>(nullable: true),
                    SimDeviceId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationState", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthenticationState_FactorCombination_FactorCombinationId",
                        column: x => x.FactorCombinationId,
                        principalTable: "FactorCombination",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthenticationState_SimDevice_SimDeviceId",
                        column: x => x.SimDeviceId,
                        principalTable: "SimDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdDeviceSettingOfflineWindowsSignIn_AdDeviceId",
                table: "AdDeviceSettingOfflineWindowsSignIn",
                column: "AdDeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationLog_FactorCombinationId",
                table: "AuthenticationLog",
                column: "FactorCombinationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationLog_SimDeviceId",
                table: "AuthenticationLog",
                column: "SimDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationLog_MultiFactorAuthenticationLogSuccess_Factor~",
                table: "AuthenticationLog",
                column: "MultiFactorAuthenticationLogSuccess_FactorCombinationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationLog_SimId",
                table: "AuthenticationLog",
                column: "SimId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationLog_SimDeviceAuthenticationLogSuccess_SimDevic~",
                table: "AuthenticationLog",
                column: "SimDeviceAuthenticationLogSuccess_SimDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationState_FactorCombinationId",
                table: "AuthenticationState",
                column: "FactorCombinationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationState_SimDeviceId",
                table: "AuthenticationState",
                column: "SimDeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Device_DomainId",
                table: "Device",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_DeviceGroupId",
                table: "Device",
                column: "DeviceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_LteId",
                table: "Device",
                column: "LteId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGroup_OrganizationId",
                table: "DeviceGroup",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Domain_OrganizationId",
                table: "Domain",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_FactorCombination_EndUserId",
                table: "FactorCombination",
                column: "EndUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FactorCombination_SimDeviceId",
                table: "FactorCombination",
                column: "SimDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Sim_SimGroupId",
                table: "Sim",
                column: "SimGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SimDevice_DeviceId",
                table: "SimDevice",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SimDevice_SimId",
                table: "SimDevice",
                column: "SimId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SimGroup_OrganizationId",
                table: "SimGroup",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_User_DomainId",
                table: "User",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_User_UserGroupId",
                table: "User",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroup_DomainId",
                table: "UserGroup",
                column: "DomainId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdDeviceSettingOfflineWindowsSignIn");

            migrationBuilder.DropTable(
                name: "AuthenticationLog");

            migrationBuilder.DropTable(
                name: "AuthenticationState");

            migrationBuilder.DropTable(
                name: "FactorCombination");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "SimDevice");

            migrationBuilder.DropTable(
                name: "UserGroup");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "Sim");

            migrationBuilder.DropTable(
                name: "Domain");

            migrationBuilder.DropTable(
                name: "DeviceGroup");

            migrationBuilder.DropTable(
                name: "Lte");

            migrationBuilder.DropTable(
                name: "SimGroup");

            migrationBuilder.DropTable(
                name: "Organization");
        }
    }
}
