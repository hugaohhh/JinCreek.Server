��CA�ؖ����Ɣ閧���쐬

```
$ docker exec -it nginx /gen-ca.sh
Generating a RSA private key
................................................+++++
.................................................................................+++++
writing new private key to '/etc/ssl/private_ca/private/cakey.pem'
Enter PEM pass phrase: �� CA�̃p�X���[�h
Verifying - Enter PEM pass phrase: �� CA�̃p�X���[�h
-----
You are about to be asked to enter information that will be incorporated
into your certificate request.
What you are about to enter is what is called a Distinguished Name or a DN.
There are quite a few fields but you can leave some blank
For some fields there will be a default value,
If you enter '.', the field will be left blank.
-----
Country Name (2 letter code) [AU]:JP
State or Province Name (full name) [Some-State]:Tokyo
Locality Name (eg, city) []:Chuo-ku
Organization Name (eg, company) [Internet Widgits Pty Ltd]:JinCreek
Organizational Unit Name (eg, section) []: (��)
Common Name (e.g. server FQDN or YOUR name) []:JinCreek CA
Email Address []: (��)
-rw-r--r-- 1 root root  917 12�� 26 16:05 /etc/ssl/private_ca/JinCreek-CA.der
-rw-r--r-- 1 root root 1298 12�� 26 16:05 /etc/ssl/private_ca/cacert.pem
```


���T�[�o�[�ؖ���: �閧���Əؖ����v���t�@�C���iCSR�j�쐬


```
$ docker exec -it nginx /gen-server.sh

common name is www-jincreek-jp

Generating a RSA private key
......+++++
..............................................................................................................................+++++
writing new private key to '/etc/ssl/server/www-jincreek-jp-key.pem'
Enter PEM pass phrase:�� �T�[�o�[�ؖ����̃p�X���[�h
Verifying - Enter PEM pass phrase:�� �T�[�o�[�ؖ����̃p�X���[�h
-----
You are about to be asked to enter information that will be incorporated
into your certificate request.
What you are about to enter is what is called a Distinguished Name or a DN.
There are quite a few fields but you can leave some blank
For some fields there will be a default value,
If you enter '.', the field will be left blank.
-----
Country Name (2 letter code) [AU]:JP
State or Province Name (full name) [Some-State]:Tokyo
Locality Name (eg, city) []:Chuo-ku
Organization Name (eg, company) [Internet Widgits Pty Ltd]:JinCreek
Organizational Unit Name (eg, section) []:(��)
Common Name (e.g. server FQDN or YOUR name) []:www.jincreek.jp
Email Address []:(��)

Please enter the following 'extra' attributes
to be sent with your certificate request
A challenge password []:(��)
An optional company name []:(��)

sign ...

Using configuration from /usr/lib/ssl/openssl.cnf
Enter pass phrase for /etc/ssl/private_ca/private/cakey.pem:�� CA�̃p�X���[�h
Check that the request matches the signature
Signature ok
Certificate Details:
        Serial Number: 1 (0x1)
        Validity
            Not Before: Dec 26 07:06:54 2019 GMT
            Not After : Dec 23 07:06:54 2029 GMT
        Subject:
            countryName               = JP
            stateOrProvinceName       = Tokyo
            organizationName          = JinCreek
            commonName                = www.jincreek.jp
        X509v3 extensions:
            X509v3 Basic Constraints:
                CA:FALSE
            Netscape Cert Type:
                SSL Client, S/MIME, Object Signing
            Netscape Comment:
                OpenSSL Generated Certificate
            X509v3 Subject Key Identifier:
                49:28:B3:D9:35:85:1B:B6:94:5A:1D:3B:18:7B:A5:38:B7:6E:1B:33
            X509v3 Authority Key Identifier:
                keyid:EC:CF:F0:87:1E:2A:47:D8:72:2C:5E:5B:9B:8B:96:9B:1C:AA:58:41

Certificate is to be certified until Dec 23 07:06:54 2029 GMT (3650 days)
Sign the certificate? [y/n]:y


1 out of 1 certificate requests certified, commit? [y/n]y
Write out database with 1 new entries
Data Base Updated

delete key-password

Enter pass phrase for /etc/ssl/server/www-jincreek-jp-key.pem:�� �T�[�o�[�ؖ����̃p�X���[�h
writing RSA key

-rw------- 1 root root 1679 12�� 26 16:07 /etc/ssl/server/www-jincreek-jp-key.pem.nopass
-rw-r--r-- 1 root root 4521 12�� 26 16:07 /etc/ssl/server/www-jincreek-jp.crt
```



���N���C�A���g�ؖ���

```
$ docker exec -it nginx /gen-client.sh

prefix is client

Generating a RSA private key
....+++++
.................+++++
writing new private key to '/etc/ssl/client/client-key.pem'
Enter PEM pass phrase:�� �N���C�A���g�ؖ����̃p�X���[�h
Verifying - Enter PEM pass phrase:�� �N���C�A���g�ؖ����̃p�X���[�h
-----
You are about to be asked to enter information that will be incorporated
into your certificate request.
What you are about to enter is what is called a Distinguished Name or a DN.
There are quite a few fields but you can leave some blank
For some fields there will be a default value,
If you enter '.', the field will be left blank.
-----
Country Name (2 letter code) [AU]:JP
State or Province Name (full name) [Some-State]:Tokyo
Locality Name (eg, city) []:Chuo-ku
Organization Name (eg, company) [Internet Widgits Pty Ltd]:JinCreek
Organizational Unit Name (eg, section) []:(��)
Common Name (e.g. server FQDN or YOUR name) []:Yamada Taro 1
Email Address []:(��)

Please enter the following 'extra' attributes
to be sent with your certificate request
A challenge password []:(��)
An optional company name []:(��)
Using configuration from /usr/lib/ssl/openssl.cnf
Enter pass phrase for /etc/ssl/private_ca/private/cakey.pem:�� CA�̃p�X���[�h
Check that the request matches the signature
Signature ok
Certificate Details:
        Serial Number: 2 (0x2)
        Validity
            Not Before: Dec 26 07:08:57 2019 GMT
            Not After : Dec 25 07:08:57 2020 GMT
        Subject:
            countryName               = JP
            stateOrProvinceName       = Tokyo
            organizationName          = JinCreek
            commonName                = Yamada Taro 1
        X509v3 extensions:
            X509v3 Basic Constraints:
                CA:FALSE
            Netscape Cert Type:
                SSL Client, S/MIME, Object Signing
            Netscape Comment:
                OpenSSL Generated Certificate
            X509v3 Subject Key Identifier:
                E6:E8:78:9B:B9:EF:37:09:50:0B:A6:8C:22:64:CB:92:14:A0:28:C9
            X509v3 Authority Key Identifier:
                keyid:EC:CF:F0:87:1E:2A:47:D8:72:2C:5E:5B:9B:8B:96:9B:1C:AA:58:41

Certificate is to be certified until Dec 25 07:08:57 2020 GMT (365 days)
Sign the certificate? [y/n]:y


1 out of 1 certificate requests certified, commit? [y/n]y
Write out database with 1 new entries
Data Base Updated

generate pfx...

Enter pass phrase for /etc/ssl/client/client-key.pem:�� �N���C�A���g�ؖ����̃p�X���[�h
Enter Export Password:�� �N���C�A���g�ؖ����{���p�̃p�X���[�h
Verifying - Enter Export Password:�N���C�A���g�ؖ����{���p�̃p�X���[�h

-rw------- 1 root root 2541 12�� 26 16:09 /client.pfx
```




CA�̃p�X���[�h: password1
�T�[�o�[�ؖ����̃p�X���[�h: password2
�N���C�A���g�ؖ����̃p�X���[�h: password3
�N���C�A���g�ؖ����{���p�̃p�X���[�h: password4

