#!/bin/bash

dir=$(cd $(dirname $0) && pwd)

branch="develop"
target_file=${branch}-Admin.zip
container="artifact"
zip_file=${dir}/roles/app-admin/files/Admin.zip

function download_zip_file() {
  AZURE_STORAGE_ACCOUNT=jincreekblobstorage \
    az storage blob download --container-name ${container} --name ${target_file} --file ${zip_file}
}

echo "Download"
download_zip_file
ls -l ${zip_file}

