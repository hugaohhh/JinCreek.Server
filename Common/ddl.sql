CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

CREATE TABLE `Lte` (
    `Id` char(36) NOT NULL,
    `LteName` VARCHAR(128) BINARY NOT NULL,
    `NwAdapterName` VARCHAR(128) BINARY NOT NULL,
    `SoftwareRadioState` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Lte` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_Lte_LteName` UNIQUE (`LteName`)
);

CREATE TABLE `Organization` (
    `Code` bigint NOT NULL AUTO_INCREMENT,
    `Name` LONGTEXT BINARY NOT NULL,
    `Address` LONGTEXT BINARY NOT NULL,
    `DelegatePhone` LONGTEXT BINARY NOT NULL,
    `Url` LONGTEXT BINARY NOT NULL,
    `AdminPhone` LONGTEXT BINARY NOT NULL,
    `AdminMail` LONGTEXT BINARY NOT NULL,
    `StartDay` DATE NOT NULL,
    `EndDay` DATE NOT NULL,
    `IsValid` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Organization` PRIMARY KEY (`Code`)
);

CREATE TABLE `DeviceGroup` (
    `Id` char(36) NOT NULL,
    `OrganizationCode` bigint NOT NULL,
    `Os` VARCHAR(128) BINARY NOT NULL,
    `Version` VARCHAR(128) BINARY NOT NULL,
    CONSTRAINT `PK_DeviceGroup` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_DeviceGroup_Code_Os_Version` UNIQUE (`OrganizationCode`, `Os`, `Version`),
    CONSTRAINT `FK_DeviceGroup_Organization_OrganizationCode` FOREIGN KEY (`OrganizationCode`) REFERENCES `Organization` (`Code`) ON DELETE CASCADE
);

CREATE TABLE `Domain` (
    `Id` char(36) NOT NULL,
    `OrganizationCode` bigint NOT NULL,
    `DomainName` VARCHAR(128) BINARY NOT NULL,
    CONSTRAINT `PK_Domain` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_Domain_DomainName` UNIQUE (`DomainName`),
    CONSTRAINT `FK_Domain_Organization_OrganizationCode` FOREIGN KEY (`OrganizationCode`) REFERENCES `Organization` (`Code`) ON DELETE CASCADE
);

CREATE TABLE `SimGroup` (
    `Id` char(36) NOT NULL,
    `OrganizationCode` bigint NOT NULL,
    `SimGroupName` VARCHAR(128) BINARY NOT NULL,
    `Nw1IpAddressPool` LONGTEXT BINARY NOT NULL,
    `Apn` LONGTEXT BINARY NOT NULL,
    `NasIpAddress` LONGTEXT BINARY NOT NULL,
    `Nw1IpAddressRange` LONGTEXT BINARY NOT NULL,
    `PrimaryDns` LONGTEXT BINARY NOT NULL,
    `SecondaryDns` LONGTEXT BINARY NOT NULL,
    `Nw1PrimaryDns` LONGTEXT BINARY NOT NULL,
    `Nw1SecondaryDns` LONGTEXT BINARY NOT NULL,
    `AuthServerIpAddress` LONGTEXT BINARY NOT NULL,
    CONSTRAINT `PK_SimGroup` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_SimGroup_Code_SimGroupName` UNIQUE (`OrganizationCode`, `SimGroupName`),
    CONSTRAINT `FK_SimGroup_Organization_OrganizationCode` FOREIGN KEY (`OrganizationCode`) REFERENCES `Organization` (`Code`) ON DELETE CASCADE
);

CREATE TABLE `Device` (
    `Id` char(36) NOT NULL,
    `DeviceName` VARCHAR(128) BINARY NOT NULL,
    `ManageNumber` LONGTEXT BINARY NOT NULL,
    `Type` LONGTEXT BINARY NOT NULL,
    `Imei` LONGTEXT BINARY NOT NULL,
    `DeviceGroupId` char(36) NOT NULL,
    `LteId` char(36) NOT NULL,
    `Discriminator` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DomainId` char(36) NULL,
    CONSTRAINT `PK_Device` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_Device_DeviceName` UNIQUE (`DeviceName`),
    CONSTRAINT `FK_Device_Domain_DomainId` FOREIGN KEY (`DomainId`) REFERENCES `Domain` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Device_DeviceGroup_DeviceGroupId` FOREIGN KEY (`DeviceGroupId`) REFERENCES `DeviceGroup` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Device_Lte_LteId` FOREIGN KEY (`LteId`) REFERENCES `Lte` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `UserGroup` (
    `Id` char(36) NOT NULL,
    `DomainId` char(36) NOT NULL,
    `UserGroupName` VARCHAR(128) BINARY NOT NULL,
    CONSTRAINT `PK_UserGroup` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_UserGroup_UserGroupName` UNIQUE (`DomainId`, `UserGroupName`),
    CONSTRAINT `FK_UserGroup_Domain_DomainId` FOREIGN KEY (`DomainId`) REFERENCES `Domain` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Sim` (
    `Id` char(36) NOT NULL,
    `SimGroupId` char(36) NOT NULL,
    `Msisdn` VARCHAR(128) BINARY NOT NULL,
    `Imsi` LONGTEXT BINARY NOT NULL,
    `IccId` LONGTEXT BINARY NOT NULL,
    `UserName` LONGTEXT BINARY NOT NULL,
    `Password` LONGTEXT BINARY NOT NULL,
    CONSTRAINT `PK_Sim` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_Sim_Msisdn` UNIQUE (`Msisdn`),
    CONSTRAINT `FK_Sim_SimGroup_SimGroupId` FOREIGN KEY (`SimGroupId`) REFERENCES `SimGroup` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AdDeviceSettingOfflineWindowsSignIn` (
    `Id` char(36) NOT NULL,
    `WindowsSignInListCacheDays` int NOT NULL,
    `AdDeviceId` char(36) NOT NULL,
    CONSTRAINT `PK_AdDeviceSettingOfflineWindowsSignIn` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AdDeviceSettingOfflineWindowsSignIn_Device_AdDeviceId` FOREIGN KEY (`AdDeviceId`) REFERENCES `Device` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `User` (
    `Id` char(36) NOT NULL,
    `AccountName` VARCHAR(128) BINARY NOT NULL,
    `Name` LONGTEXT BINARY NULL,
    `UserDiscriminator` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DomainId` char(36) NULL,
    `UserGroupId` char(36) NULL,
    `IsDisconnectWhenScreenLock` tinyint(1) NULL,
    `Password` longtext CHARACTER SET utf8mb4 NULL,
    `SuperAdminUser_Password` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_User` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_User_AccountName` UNIQUE (`AccountName`),
    CONSTRAINT `FK_User_Domain_DomainId` FOREIGN KEY (`DomainId`) REFERENCES `Domain` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_User_UserGroup_UserGroupId` FOREIGN KEY (`UserGroupId`) REFERENCES `UserGroup` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `SimDevice` (
    `Id` char(36) NOT NULL,
    `SimId` char(36) NOT NULL,
    `DeviceId` char(36) NOT NULL,
    `Nw2IpAddressPool` LONGTEXT BINARY NOT NULL,
    `AuthPeriod` int NOT NULL,
    `StartDay` datetime(6) NOT NULL,
    `EndDay` datetime(6) NOT NULL,
    CONSTRAINT `PK_SimDevice` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_SimDevice_SimId_DeviceId` UNIQUE (`SimId`, `DeviceId`),
    CONSTRAINT `FK_SimDevice_Device_DeviceId` FOREIGN KEY (`DeviceId`) REFERENCES `Device` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_SimDevice_Sim_SimId` FOREIGN KEY (`SimId`) REFERENCES `Sim` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `FactorCombination` (
    `Id` char(36) NOT NULL,
    `SimDeviceId` char(36) NOT NULL,
    `EndUserId` char(36) NOT NULL,
    `NwIpAddress` LONGTEXT BINARY NOT NULL,
    `StartDay` datetime(6) NOT NULL,
    `EndDay` datetime(6) NULL,
    CONSTRAINT `PK_FactorCombination` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_MultiFactor_SimDeviceId_EndUserId` UNIQUE (`SimDeviceId`, `EndUserId`),
    CONSTRAINT `FK_FactorCombination_User_EndUserId` FOREIGN KEY (`EndUserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_FactorCombination_SimDevice_SimDeviceId` FOREIGN KEY (`SimDeviceId`) REFERENCES `SimDevice` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AuthenticationLog` (
    `Id` char(36) NOT NULL,
    `ConnectionTime` datetime(6) NOT NULL,
    `AuthenticationLogDiscriminator` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FactorCombinationId` char(36) NULL,
    `SimDeviceId` char(36) NULL,
    `MultiFactorAuthenticationLogSuccess_FactorCombinationId` char(36) NULL,
    `SimId` char(36) NULL,
    `SimDeviceAuthenticationLogSuccess_SimDeviceId` char(36) NULL,
    CONSTRAINT `PK_AuthenticationLog` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AuthenticationLog_FactorCombination_FactorCombinationId` FOREIGN KEY (`FactorCombinationId`) REFERENCES `FactorCombination` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AuthenticationLog_SimDevice_SimDeviceId` FOREIGN KEY (`SimDeviceId`) REFERENCES `SimDevice` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AuthenticationLog_FactorCombination_MultiFactorAuthenticatio~` FOREIGN KEY (`MultiFactorAuthenticationLogSuccess_FactorCombinationId`) REFERENCES `FactorCombination` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AuthenticationLog_Sim_SimId` FOREIGN KEY (`SimId`) REFERENCES `Sim` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AuthenticationLog_SimDevice_SimDeviceAuthenticationLogSucces~` FOREIGN KEY (`SimDeviceAuthenticationLogSuccess_SimDeviceId`) REFERENCES `SimDevice` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AuthenticationState` (
    `Id` char(36) NOT NULL,
    `TimeLimit` datetime(6) NOT NULL,
    `AuthenticationStateDiscriminator` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FactorCombinationId` char(36) NULL,
    `SimDeviceId` char(36) NULL,
    CONSTRAINT `PK_AuthenticationState` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AuthenticationState_FactorCombination_FactorCombinationId` FOREIGN KEY (`FactorCombinationId`) REFERENCES `FactorCombination` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AuthenticationState_SimDevice_SimDeviceId` FOREIGN KEY (`SimDeviceId`) REFERENCES `SimDevice` (`Id`) ON DELETE CASCADE
);

CREATE UNIQUE INDEX `IX_AdDeviceSettingOfflineWindowsSignIn_AdDeviceId` ON `AdDeviceSettingOfflineWindowsSignIn` (`AdDeviceId`);

CREATE INDEX `IX_AuthenticationLog_FactorCombinationId` ON `AuthenticationLog` (`FactorCombinationId`);

CREATE INDEX `IX_AuthenticationLog_SimDeviceId` ON `AuthenticationLog` (`SimDeviceId`);

CREATE INDEX `IX_AuthenticationLog_MultiFactorAuthenticationLogSuccess_Factor~` ON `AuthenticationLog` (`MultiFactorAuthenticationLogSuccess_FactorCombinationId`);

CREATE INDEX `IX_AuthenticationLog_SimId` ON `AuthenticationLog` (`SimId`);

CREATE INDEX `IX_AuthenticationLog_SimDeviceAuthenticationLogSuccess_SimDevic~` ON `AuthenticationLog` (`SimDeviceAuthenticationLogSuccess_SimDeviceId`);

CREATE UNIQUE INDEX `IX_AuthenticationState_FactorCombinationId` ON `AuthenticationState` (`FactorCombinationId`);

CREATE UNIQUE INDEX `IX_AuthenticationState_SimDeviceId` ON `AuthenticationState` (`SimDeviceId`);

CREATE INDEX `IX_Device_DomainId` ON `Device` (`DomainId`);

CREATE INDEX `IX_Device_DeviceGroupId` ON `Device` (`DeviceGroupId`);

CREATE INDEX `IX_Device_LteId` ON `Device` (`LteId`);

CREATE INDEX `IX_Domain_OrganizationCode` ON `Domain` (`OrganizationCode`);

CREATE INDEX `IX_FactorCombination_EndUserId` ON `FactorCombination` (`EndUserId`);

CREATE INDEX `IX_Sim_SimGroupId` ON `Sim` (`SimGroupId`);

CREATE UNIQUE INDEX `IX_SimDevice_DeviceId` ON `SimDevice` (`DeviceId`);

CREATE UNIQUE INDEX `IX_SimDevice_SimId` ON `SimDevice` (`SimId`);

CREATE INDEX `IX_User_DomainId` ON `User` (`DomainId`);

CREATE INDEX `IX_User_UserGroupId` ON `User` (`UserGroupId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20200120074803_IntialCreate', '3.1.0');


