INSERT INTO Company (company_name) VALUES ('ジンクリーク社');
INSERT INTO Company (company_name) VALUES ('ジンクリーク社2');

INSERT INTO Domain (domain_name, code) VALUES ('jincreek.jp', (SELECT code from Company where company_name = 'ジンクリーク社'));
INSERT INTO Domain (domain_name, code) VALUES ('jincreek21.com', (SELECT code from Company where company_name = 'ジンクリーク社2'));
INSERT INTO Domain (domain_name, code) VALUES ('jincreek22.com', (SELECT code from Company where company_name = 'ジンクリーク社2'));

INSERT INTO `Group` (domain_name, group_name) VALUES ((SELECT domain_name from Domain where code = (SELECT code from Company where company_name = 'ジンクリーク社')), 'group1');

INSERT INTO User (account_name, domain_name, group_name, last_name, first_name) VALUES ('testuser', (SELECT domain_name from Domain where code = (SELECT code from Company where company_name = 'ジンクリーク社')), 'group1', '姓', '名');

INSERT INTO SimGroup(apn, code, addr1_pool_name) VALUES ('indigoapn', (SELECT code from Company where company_name = 'ジンクリーク社'), 'indigo_pool');

INSERT INTO Sim(msisdn, apn, imsi, icc_id, user_name, password) VALUES ('02011110000', (SELECT apn from SimGroup where code = (SELECT code from Company where company_name = 'ジンクリーク社')), '440103213100000', '8981100005811110000', 'test5@tripodworks2', 'pass12345');

INSERT INTO Os(code, type, version) VALUES ((SELECT code from Company where company_name = 'ジンクリーク社'), 'Windows 10 Pro', '18362.418');

INSERT INTO Device(imei, type, version, internal_id, model, serial_no) VALUES ('356730085724666', (SELECT type from Os where code = (SELECT code from Company where company_name = 'ジンクリーク社')), (SELECT version from Os where code = (SELECT code from Company where company_name = 'ジンクリーク社')), 'MNG001', 'DELL Latitude 7290', '4EB8E852-B10C-4042-9E06-9B75A4483DE2');

INSERT INTO FactorCombination(imei, msisdn, account_name, addr2, addr3, auth_period, start_date, end_date, is_available) VALUES ((SELECT imei from Device where (type, version) = (SELECT type, version from Os where code = (SELECT code from Company where company_name = 'ジンクリーク社'))), (SELECT msisdn from Sim where apn = (SELECT apn from SimGroup where code = (SELECT code from Company where company_name = 'ジンクリーク社'))), (SELECT account_name from User where (domain_name, group_name) = (SELECT domain_name, group_name from `Group` where domain_name = (SELECT domain_name from Domain where code = (SELECT code from Company where company_name = 'ジンクリーク社')))), '192.168.0.100/32', '192.168.0.200/32', 100, now(), now(), true);


INSERT INTO Connect (imei, msisdn, account_name, connect_status, expiration_date) VALUES ((SELECT imei from Device where (type, version) = (SELECT type, version from Os where code = (SELECT code from Company where company_name = 'ジンクリーク社'))), (SELECT msisdn from Sim where apn = (SELECT apn from SimGroup where code = (SELECT code from Company where company_name = 'ジンクリーク社'))), (SELECT account_name from User where (domain_name, group_name) = (SELECT domain_name, group_name from `Group` where domain_name = (SELECT domain_name from Domain where code = (SELECT code from Company where company_name = 'ジンクリーク社')))), 1, str_to_date('2019-12-01 23:59:59', '%Y-%m-%d %H:%i:%s'));

INSERT INTO ConnectLog (imei, msisdn, account_name, connect_status, is_auth_result, connect_date, send_byte, receive_byte) VALUES ((SELECT imei from Device where (type, version) = (SELECT type, version from Os where code = (SELECT code from Company where company_name = 'ジンクリーク社'))), (SELECT msisdn from Sim where apn = (SELECT apn from SimGroup where code = (SELECT code from Company where company_name = 'ジンクリーク社'))), (SELECT account_name from User where (domain_name, group_name) = (SELECT domain_name, group_name from `Group` where domain_name = (SELECT domain_name from Domain where code = (SELECT code from Company where company_name = 'ジンクリーク社')))), 1, 1, now(), 100, 200);

