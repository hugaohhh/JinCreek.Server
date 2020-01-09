CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

CREATE TABLE `Lte` (
    `Id` char(36) NOT NULL,
    `LteName` LONGTEXT BINARY NOT NULL,
    `LteAdapter` LONGTEXT BINARY NOT NULL,
    `SoftwareRadioState` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Lte` PRIMARY KEY (`Id`)
);

CREATE TABLE `Organization` (
    `Id` char(36) NOT NULL,
    `Code` VARCHAR(255) BINARY NOT NULL,
    `Name` LONGTEXT BINARY NULL,
    `Address` LONGTEXT BINARY NULL,
    `DelegatePhone` LONGTEXT BINARY NULL,
    `AdminPhone` LONGTEXT BINARY NULL,
    `AdminMail` LONGTEXT BINARY NULL,
    `StartDay` datetime(6) NOT NULL,
    `EndDay` datetime(6) NOT NULL,
    `Url` LONGTEXT BINARY NULL,
    `IsValid` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Organization` PRIMARY KEY (`Id`),
    CONSTRAINT `Organization_Code_UQ` UNIQUE (`Code`)
);

CREATE TABLE `DeviceGroup` (
    `Id` char(36) NOT NULL,
    `Version` LONGTEXT BINARY NOT NULL,
    `OsType` LONGTEXT BINARY NOT NULL,
    `OrganizationId` char(36) NOT NULL,
    CONSTRAINT `PK_DeviceGroup` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DeviceGroup_Organization_OrganizationId` FOREIGN KEY (`OrganizationId`) REFERENCES `Organization` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Domain` (
    `Id` char(36) NOT NULL,
    `DomainName` LONGTEXT BINARY NOT NULL,
    `OrganizationId` char(36) NOT NULL,
    CONSTRAINT `PK_Domain` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Domain_Organization_OrganizationId` FOREIGN KEY (`OrganizationId`) REFERENCES `Organization` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `SimGroup` (
    `Id` char(36) NOT NULL,
    `SimGroupName` LONGTEXT BINARY NOT NULL,
    `OrganizationId` char(36) NOT NULL,
    `PrimaryDns` LONGTEXT BINARY NOT NULL,
    `SecondDns` LONGTEXT BINARY NOT NULL,
    `Apn` LONGTEXT BINARY NOT NULL,
    `NasAddress` LONGTEXT BINARY NOT NULL,
    `Nw1AddressPool` LONGTEXT BINARY NOT NULL,
    `Nw1AddressRange` LONGTEXT BINARY NOT NULL,
    `ServerAddress` LONGTEXT BINARY NOT NULL,
    CONSTRAINT `PK_SimGroup` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_SimGroup_Organization_OrganizationId` FOREIGN KEY (`OrganizationId`) REFERENCES `Organization` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Device` (
    `Id` char(36) NOT NULL,
    `DeviceName` LONGTEXT BINARY NOT NULL,
    `MakeNumber` LONGTEXT BINARY NOT NULL,
    `ManagerNumber` LONGTEXT BINARY NOT NULL,
    `Type` LONGTEXT BINARY NOT NULL,
    `DeviceGroupId` char(36) NOT NULL,
    `LteId` char(36) NOT NULL,
    `Discriminator` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DomainId` char(36) NULL,
    CONSTRAINT `PK_Device` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Device_Domain_DomainId` FOREIGN KEY (`DomainId`) REFERENCES `Domain` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Device_DeviceGroup_DeviceGroupId` FOREIGN KEY (`DeviceGroupId`) REFERENCES `DeviceGroup` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Device_Lte_LteId` FOREIGN KEY (`LteId`) REFERENCES `Lte` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `UserGroup` (
    `Id` char(36) NOT NULL,
    `UserGroupName` LONGTEXT BINARY NOT NULL,
    `DomainId` char(36) NOT NULL,
    CONSTRAINT `PK_UserGroup` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserGroup_Domain_DomainId` FOREIGN KEY (`DomainId`) REFERENCES `Domain` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Sim` (
    `Id` char(36) NOT NULL,
    `Msisdn` LONGTEXT BINARY NOT NULL,
    `Imsi` LONGTEXT BINARY NOT NULL,
    `IccId` LONGTEXT BINARY NOT NULL,
    `UserName` LONGTEXT BINARY NOT NULL,
    `Password` LONGTEXT BINARY NOT NULL,
    `SimGroupId` char(36) NOT NULL,
    CONSTRAINT `PK_Sim` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Sim_SimGroup_SimGroupId` FOREIGN KEY (`SimGroupId`) REFERENCES `SimGroup` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `DeviceSetting` (
    `Id` char(36) NOT NULL,
    `IsOffLineWindowsSingIn` tinyint(1) NOT NULL,
    `WindowsSignInListCacheDays` int NOT NULL,
    `AdDeviceId` char(36) NOT NULL,
    CONSTRAINT `PK_DeviceSetting` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DeviceSetting_Device_AdDeviceId` FOREIGN KEY (`AdDeviceId`) REFERENCES `Device` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `User` (
    `Id` char(36) NOT NULL,
    `DomainId` char(36) NOT NULL,
    `UserGroupId` char(36) NOT NULL,
    `LastName` LONGTEXT BINARY NULL,
    `FirstName` LONGTEXT BINARY NULL,
    `SettingByUser` longtext CHARACTER SET utf8mb4 NULL,
    `UserType` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Password` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_User` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_User_Domain_DomainId` FOREIGN KEY (`DomainId`) REFERENCES `Domain` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_User_UserGroup_UserGroupId` FOREIGN KEY (`UserGroupId`) REFERENCES `UserGroup` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `SimDevice` (
    `Id` char(36) NOT NULL,
    `SimId` char(36) NOT NULL,
    `DeviceId` char(36) NOT NULL,
    `Nw2AddressPool` LONGTEXT BINARY NOT NULL,
    `StartDay` datetime(6) NOT NULL,
    `EndDay` datetime(6) NOT NULL,
    `AuthPeriod` int NOT NULL,
    CONSTRAINT `PK_SimDevice` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_SimDevice_Device_DeviceId` FOREIGN KEY (`DeviceId`) REFERENCES `Device` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_SimDevice_Sim_SimId` FOREIGN KEY (`SimId`) REFERENCES `Sim` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `UserSetting` (
    `Id` char(36) NOT NULL,
    `IsDisconnectWhenScreenLock` tinyint(1) NOT NULL,
    `UserId` char(36) NOT NULL,
    CONSTRAINT `PK_UserSetting` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserSetting_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `FactorCombination` (
    `Id` char(36) NOT NULL,
    `SimDeviceId` char(36) NOT NULL,
    `UserId` char(36) NOT NULL,
    `NwAddress` LONGTEXT BINARY NOT NULL,
    `StartDay` datetime(6) NOT NULL,
    `EndDay` datetime(6) NOT NULL,
    CONSTRAINT `PK_FactorCombination` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_FactorCombination_SimDevice_SimDeviceId` FOREIGN KEY (`SimDeviceId`) REFERENCES `SimDevice` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_FactorCombination_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AuthenticationLog` (
    `Id` char(36) NOT NULL,
    `ConnectionTime` datetime(6) NOT NULL,
    `SendByte` int NOT NULL,
    `ReceviByte` int NOT NULL,
    `Discriminator` longtext CHARACTER SET utf8mb4 NOT NULL,
    `IsAuthResult` tinyint(1) NULL,
    `MultiFactorAuthentication_FactorCombinationId` char(36) NULL,
    `SimDeviceAuth` int NULL,
    `SimDeviceId` char(36) NULL,
    `FactorCombinationId` char(36) NULL,
    CONSTRAINT `PK_AuthenticationLog` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AuthenticationLog_FactorCombination_FactorCombinationId` FOREIGN KEY (`FactorCombinationId`) REFERENCES `FactorCombination` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AuthenticationLog_FactorCombination_MultiFactorAuthenticatio~` FOREIGN KEY (`MultiFactorAuthentication_FactorCombinationId`) REFERENCES `FactorCombination` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AuthenticationLog_SimDevice_SimDeviceId` FOREIGN KEY (`SimDeviceId`) REFERENCES `SimDevice` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AuthenticationState` (
    `Id` char(36) NOT NULL,
    `TimeLimit` datetime(6) NOT NULL,
    `Discriminator` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FactorCombinationId` char(36) NULL,
    `SimDeviceId` char(36) NULL,
    CONSTRAINT `PK_AuthenticationState` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AuthenticationState_FactorCombination_FactorCombinationId` FOREIGN KEY (`FactorCombinationId`) REFERENCES `FactorCombination` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AuthenticationState_SimDevice_SimDeviceId` FOREIGN KEY (`SimDeviceId`) REFERENCES `SimDevice` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_AuthenticationLog_FactorCombinationId` ON `AuthenticationLog` (`FactorCombinationId`);

CREATE INDEX `IX_AuthenticationLog_MultiFactorAuthentication_FactorCombinatio~` ON `AuthenticationLog` (`MultiFactorAuthentication_FactorCombinationId`);

CREATE INDEX `IX_AuthenticationLog_SimDeviceId` ON `AuthenticationLog` (`SimDeviceId`);

CREATE UNIQUE INDEX `IX_AuthenticationState_FactorCombinationId` ON `AuthenticationState` (`FactorCombinationId`);

CREATE UNIQUE INDEX `IX_AuthenticationState_SimDeviceId` ON `AuthenticationState` (`SimDeviceId`);

CREATE INDEX `IX_Device_DomainId` ON `Device` (`DomainId`);

CREATE INDEX `IX_Device_DeviceGroupId` ON `Device` (`DeviceGroupId`);

CREATE INDEX `IX_Device_LteId` ON `Device` (`LteId`);

CREATE INDEX `IX_DeviceGroup_OrganizationId` ON `DeviceGroup` (`OrganizationId`);

CREATE UNIQUE INDEX `IX_DeviceSetting_AdDeviceId` ON `DeviceSetting` (`AdDeviceId`);

CREATE INDEX `IX_Domain_OrganizationId` ON `Domain` (`OrganizationId`);

CREATE INDEX `IX_FactorCombination_SimDeviceId` ON `FactorCombination` (`SimDeviceId`);

CREATE INDEX `IX_FactorCombination_UserId` ON `FactorCombination` (`UserId`);

CREATE INDEX `IX_Sim_SimGroupId` ON `Sim` (`SimGroupId`);

CREATE UNIQUE INDEX `IX_SimDevice_DeviceId` ON `SimDevice` (`DeviceId`);

CREATE UNIQUE INDEX `IX_SimDevice_SimId` ON `SimDevice` (`SimId`);

CREATE INDEX `IX_SimGroup_OrganizationId` ON `SimGroup` (`OrganizationId`);

CREATE INDEX `IX_User_DomainId` ON `User` (`DomainId`);

CREATE INDEX `IX_User_UserGroupId` ON `User` (`UserGroupId`);

CREATE INDEX `IX_UserGroup_DomainId` ON `UserGroup` (`DomainId`);

CREATE UNIQUE INDEX `IX_UserSetting_UserId` ON `UserSetting` (`UserId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20200109013414_InitialCreate', '3.1.0');

