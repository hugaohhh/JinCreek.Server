-- CREATE DATABASE `jincreekindigo`;
-- GRANT ALL PRIVILEGES ON jincreekindigo.* TO jincreekr002demo@localhost;
-- GRANT ALL PRIVILEGES ON jincreekindigo.* TO jincreekr002demo@'192.168.5.1/255.255.255.255' IDENTIFIED BY 'jincreekr002demo';
-- GRANT ALL PRIVILEGES ON jincreekindigo.* TO jincreekr002demo@_gateway IDENTIFIED BY 'jincreekr002demo';

-- 企業
CREATE TABLE Company (
	  `code` int(11) NOT NULL AUTO_INCREMENT,
	  `company_name` text NOT NULL,
	  PRIMARY KEY (`code`)
);

-- 企業別設定
CREATE TABLE CompanySetting (
	  `code` int(11) NOT NULL,
	  `api_addr` varchar(18) NOT NULL,
          `is_lock_screen_disconnect` boolean NOT NULL,
	  CONSTRAINT `CompanySetting_FK1` FOREIGN KEY (`code`) REFERENCES `Company` (`code`)
);

-- ドメイン
CREATE TABLE Domain (
	  `domain_name` varchar(128) NOT NULL,
	  `code` int(11) NOT NULL,
	  PRIMARY KEY (`domain_name`),
	  CONSTRAINT `Domain_FK1` FOREIGN KEY (`code`) REFERENCES `Company` (`code`)
);

-- グループ
CREATE TABLE `Group` (
	  `domain_name` varchar(128) NOT NULL,
	  `group_name` varchar(128) NOT NULL,
	  PRIMARY KEY (`domain_name`, `group_name`),
	  CONSTRAINT `Group_FK1` FOREIGN KEY (`domain_name`) REFERENCES `Domain` (`domain_name`)
);

-- ユーザー
CREATE TABLE `User` (
	  `account_name` varchar(128) NOT NULL,
	  `domain_name` varchar(128) NOT NULL,
	  `group_name` varchar(128) NOT NULL,
	  `last_name` varchar(128) NULL,
	  `first_name` varchar(128) NULL,
	  PRIMARY KEY (`account_name`),
	  CONSTRAINT `User_FK1` FOREIGN KEY (`domain_name`, `group_name`) REFERENCES `Group` (`domain_name`, `group_name`)
);

-- SIMグループ
CREATE TABLE `SimGroup` (
	  `apn` varchar(128) NOT NULL,
	  `code` int(11) NOT NULL,
	  `addr1_pool_name` varchar(128) NOT NULL,
	  `default_addr1` varchar(18) NULL,
	  PRIMARY KEY (`apn`),
	  CONSTRAINT `SimGroup_FK1` FOREIGN KEY (`code`) REFERENCES `Company` (`code`)
);

-- SIM
CREATE TABLE `Sim` (
	  `msisdn` varchar(128) NOT NULL,
	  `apn` varchar(128) NOT NULL,
	  `imsi` varchar(128) NOT NULL,
	  `icc_id` varchar(128) NOT NULL,
	  `user_name` varchar(128) NOT NULL,
	  `password` varchar(128) NOT NULL,
	  PRIMARY KEY (`msisdn`),
	  CONSTRAINT `Sim_FK1` FOREIGN KEY (`apn`) REFERENCES `SimGroup` (`apn`)
);

-- OSバージョン
CREATE TABLE `Os` (
	  `code` int(11) NOT NULL,
          `type` varchar(128) NOT NULL,
          `version` varchar(128) NOT NULL,
	  PRIMARY KEY (`type`, `version`),
	  CONSTRAINT `Os_FK1` FOREIGN KEY (`code`) REFERENCES `Company` (`code`)
);

-- Device
CREATE TABLE `Device` (
          `imei` varchar(128) NOT NULL,
          `type` varchar(128) NOT NULL,
          `version` varchar(128) NOT NULL,
          `internal_id` varchar(128) NOT NULL,
          `model` varchar(128) NOT NULL,
          `serial_no` varchar(128) NOT NULL,
	  PRIMARY KEY (`imei`),
	  CONSTRAINT `Device_FK1` FOREIGN KEY (`type`, `version`) REFERENCES `Os` (`type`, `version`)
);

-- 認証要素組み合わせ
CREATE TABLE `FactorCombination` (
          `imei` varchar(128) NOT NULL,
	  `msisdn` varchar(128) NOT NULL,
	  `account_name` varchar(128) NOT NULL,
	  `addr2` varchar(18) NOT NULL,
	  `addr3` varchar(18) NOT NULL,
	  `auth_period` int(11) NOT NULL,
	  `start_date` datetime NOT NULL,
	  `end_date` datetime NOT NULL,
	  `is_available` boolean NOT NULL,
	  PRIMARY KEY (`imei`, `msisdn`, `account_name`),
 	  CONSTRAINT `FactorCombination_FK1` FOREIGN KEY (`imei`) REFERENCES `Device` (`imei`),
 	  CONSTRAINT `FactorCombination_FK2` FOREIGN KEY (`msisdn`) REFERENCES `Sim` (`msisdn`),
 	  CONSTRAINT `FactorCombination_FK3` FOREIGN KEY (`account_name`) REFERENCES `User` (`account_name`)
);

-- 接続
CREATE TABLE `Connect` (
          `imei` varchar(128) NOT NULL,
	  `msisdn` varchar(128) NOT NULL,
	  `account_name` varchar(128) NOT NULL,
          `connect_status` int(1) NOT NULL,
          `expiration_date` datetime NOT NULL,
	  PRIMARY KEY (`imei`, `msisdn`, `account_name`),
 	  CONSTRAINT `Connect_FK1` FOREIGN KEY (`imei`, `msisdn`, `account_name`) REFERENCES `FactorCombination` (`imei`, `msisdn`, `account_name`)
);

-- 接続ログ
CREATE TABLE `ConnectLog` (
          `connect_log_id` bigint NOT NULL AUTO_INCREMENT,
          `imei` varchar(128) NOT NULL,
	  `msisdn` varchar(128) NOT NULL,
	  `account_name` varchar(128) NOT NULL,
          `connect_status` int(1) NOT NULL,
          `is_auth_result` boolean NOT NULL,
          `connect_date` datetime NOT NULL,
          `send_byte` int(11),
          `receive_byte` int(11),
	  PRIMARY KEY (`connect_log_id`),
 	  CONSTRAINT `ConnectLog_FK1` FOREIGN KEY (`imei`, `msisdn`, `account_name`) REFERENCES `FactorCombination` (`imei`, `msisdn`, `account_name`)
);




