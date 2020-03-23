
-- SuperAdminUser
INSERT INTO `mdb`.`User` (`Id`, `AccountName`, `Name`, `UserDiscriminator`, `SuperAdminUser_Password`) VALUES ('ce7c6b21-d762-4127-974d-8169ff257c34', 'USER0', 'user0', 'superAdmin', 'ANw1EQRZy4xKUUcvK8FBjxRjCsQbzKM7jaI+Wh12vEe5Ulwco3K1Ez9+1bTfdD4tCw==');


INSERT INTO `mdb`.`Organization` (`Code`, `Name`, `Address`, `DelegatePhone`, `Url`, `AdminPhone`, `AdminMail`, `StartDay`, `IsValid`) VALUES ('1', 'org1', 'aaabbbccc', '09011112222', 'http://example.com', '09011113333', 'aaa@example.com', '2020-01-09', '1');
INSERT INTO `mdb`.`Domain` (`Id`, `OrganizationCode`, `DomainName`) VALUES ('38a02f03-0491-47a5-be23-1f42d25fae73', '1', 'domain1');
INSERT INTO `mdb`.`UserGroup` (`Id`, `DomainId`, `UserGroupName`) VALUES ('85cb4733-8a84-455e-bbab-1dbd5f5e5eb7', '38a02f03-0491-47a5-be23-1f42d25fae73', 'userGroup1');
INSERT INTO `mdb`.`User` (`Id`, `AccountName`, `Name`, `UserDiscriminator`, `DomainId`, `Password`) VALUES ('d6196612-01bf-474f-9db3-695f78f5dc27', 'USER1', 'user1', 'admin', '38a02f03-0491-47a5-be23-1f42d25fae73', 'AHqmBDXYEBf3BeVuo3W8AmPPaQkqoSaWzes3jfYKQJ8udIAjhjuCAn9F+Fe4rcq8dw==');
INSERT INTO `mdb`.`UserGroupEndUser` (`Id`, `UserGroupId`, `EndUserId`) VALUES ('9c3dba88-fdb5-4ed9-9a61-4d7d1d63c379', '85cb4733-8a84-455e-bbab-1dbd5f5e5eb7', 'd6196612-01bf-474f-9db3-695f78f5dc27');
INSERT INTO `mdb`.`AvailablePeriod` (`Id`, `EndUserId`, `StartDay`, `EndDay`) VALUES ('95297dca-29e1-4016-88c3-ed823dd0ef5e', 'd6196612-01bf-474f-9db3-695f78f5dc27', '2020-02-03', '2021-02-03');


-- devices
INSERT INTO `mdb`.`DeviceGroup` (`Id`, `DomainId`, `DeviceGroupName`, `OrganizationCode`) VALUES ('a8cb2782-5211-45df-9fcb-1b18b89cc61e', '38a02f03-0491-47a5-be23-1f42d25fae73', 'deviceGroup1', '1');
INSERT INTO `mdb`.`DeviceGroup` (`Id`, `DomainId`, `DeviceGroupName`, `OrganizationCode`) VALUES ('e0c8d420-c8bd-434b-b5d1-690340973495', '38a02f03-0491-47a5-be23-1f42d25fae73', 'deviceGroup2', '1');
INSERT INTO `mdb`.`Device` (`Id`, `DomainId`, `DeviceName`, `DeviceGroupId`, `ManageNumber`, `SerialNumber`, `ProductName`, `Tpm`, `WindowsSignInListCacheDays`) VALUES ('4258a5b4-fe85-443f-92e9-6ae15fb9fa3d', '38a02f03-0491-47a5-be23-1f42d25fae73', 'device1', 'a8cb2782-5211-45df-9fcb-1b18b89cc61e', '1', '', '', '0', '0');
