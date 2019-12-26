#!/bin/bash

dir=$(cd $(dirname ${0}) && pwd)

ca_dir=/etc/ssl/private_ca
expire=3650


openssl req -new -x509 -newkey rsa:2048 \
	-out ${ca_dir}/cacert.pem -keyout ${ca_dir}/private/cakey.pem -days ${expire}


openssl x509 -inform PEM -outform DER -in ${ca_dir}/cacert.pem -out ${ca_dir}/JinCreek-CA.der

ls -l ${ca_dir}/cacert.pem ${ca_dir}/JinCreek-CA.der

