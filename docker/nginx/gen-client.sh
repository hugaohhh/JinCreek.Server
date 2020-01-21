#!/bin/bash

dir=$(cd $(dirname ${0}) && pwd)

ca_dir=/etc/ssl/private_ca
clt_dir=/etc/ssl/client

expire=3650

prefix=${1:-client}

csr=${clt_dir}/${prefix}-csr.pem
key=${clt_dir}/${prefix}-key.pem
crt=${clt_dir}/${prefix}.crt
pfx=${ctl_dir}/${prefix}.pfx
pem=${ctl_dir}/${prefix}.pem

echo ""
echo "prefix is ${prefix}"
echo ""
openssl req -new -sha256 -keyout ${key} -out ${csr}

openssl ca -md sha256 \
	-cert ${ca_dir}/cacert.pem -keyfile ${ca_dir}/private/cakey.pem \
	-out ${crt} -infiles ${csr}

echo ""
echo "generate pfx..."
echo ""
openssl pkcs12 -export -in ${crt} -inkey ${key} \
	-out ${pfx} #-name "www.jincreek.jp"

echo ""
ls -l ${pfx}

openssl pkcs12 -in ${pfx} -out ${pem} -nodes -clcerts


