#!/bin/bash

dir=$(cd $(dirname $0) && pwd)

name="batch"

app_dir=/opt/apps
log_dir=${app_dir}/logs

app_dll=${app_dir}/Batch/Batch.dll
log_file=${log_dir}/${name}.log


pushd ${app_dir}/Batch
#nohup dotnet ${app_dll} $@ >> ${log_file} 2>&1
${app_dir}/Batch/Batch $@
popd
