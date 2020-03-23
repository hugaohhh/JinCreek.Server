#!/bin/bash

dir=$(cd $(dirname $0) && pwd)

name="admin"

app_dir=/opt/apps
run_dir=/var/run/admin
log_dir=${app_dir}/logs

app_dll=${app_dir}/Admin/Admin.dll
pid_file=${run_dir}/${name}.pid
log_file=${log_dir}/${name}.log
log_file_backup=${log_dir}/${name}_snapshot_$(date '+%Y%m%d_%H%M%S').log


if [ -f "${pid_file}" ]; then
  kill -9 $(cat ${pid_file})
fi

if [ -f "${log_file}" ]; then
  mv ${log_file} ${log_file_backup}
  gzip -9 ${log_file_backup}
fi

pushd ${app_dir}/Admin
nohup dotnet ${app_dll} >> ${log_file} 2>&1  & 
echo $! > ${pid_file}
popd
