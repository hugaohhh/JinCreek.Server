#!/bin/bash

dir=$(cd $(dirname ${0}) && pwd)

ca_dir=/etc/ssl/private_ca
svr_dir=/etc/ssl/server

expire=3650

common_name=${1:-www-jincreek-jp}

csr=${svr_dir}/${common_name}-csr.pem
key=${svr_dir}/${common_name}-key.pem
crt=${svr_dir}/${common_name}.crt
key_nopass=${svr_dir}/${common_name}-key.pem.nopass


echo ""
echo "common name is ${common_name}"
echo ""
openssl req -new -keyout ${key} -out ${csr}


echo ""
echo "sign ... "
echo ""
openssl ca -days ${expire} \
	-cert    ${ca_dir}/cacert.pem \
	-keyfile ${ca_dir}/private/cakey.pem \
	-in ${csr} > ${crt}


echo ""
echo "delete key-password"
echo ""
openssl rsa -in ${key} -out ${key_nopass}

echo ""
ls -l ${crt} ${key_nopass}


