# Server.csproj


## ユーザ登録

例：

```
$ http -v --verify=no POST https://localhost:5001/api/authentication/register username="t-suzuki@indigo.co.jp" password="9Q'vl!"
POST /api/authentication/register HTTP/1.1
Accept: application/json, */*
Accept-Encoding: gzip, deflate
Connection: keep-alive
Content-Length: 59
Content-Type: application/json
Host: localhost:5001
User-Agent: HTTPie/1.0.3

{
    "password": "9Q'vl!",
    "username": "t-suzuki@indigo.co.jp"
}

HTTP/1.1 200 OK
Content-Length: 0
Date: Fri, 20 Dec 2019 05:18:57 GMT
Server: Kestrel
```


## トークン取得

例：

```
$ http -v --verify=no POST https://localhost:5001/api/authentication/login username="t-suzuki@indigo.co.jp" password="9Q'vl!"
POST /api/authentication/login HTTP/1.1
Accept: application/json, */*
Accept-Encoding: gzip, deflate
Connection: keep-alive
Content-Length: 59
Content-Type: application/json
Host: localhost:5001
User-Agent: HTTPie/1.0.3

{
    "password": "9Q'vl!",
    "username": "t-suzuki@indigo.co.jp"
}

HTTP/1.1 200 OK
Cache-Control: no-cache
Content-Type: application/json; charset=utf-8
Date: Fri, 20 Dec 2019 05:20:29 GMT
Expires: Thu, 01 Jan 1970 00:00:00 GMT
Pragma: no-cache
Server: Kestrel
Set-Cookie: .AspNetCore.Identity.Application=CfDJ8GdXAJiHVDlMk2THvgbmyBerq2PL-MlbhYVESPUrXALq5fAJ6YU6J-HZHP-DFllBBfU48_dwG1UhrrnKxnYz4GNU57S5q5VUMs4mQs6HpvLLUvIz-TCsIDYilZf-Uu49dn9byRGsthSWFws1w7EcZiimxqSFOG6tK0XIgjxQB_rT9Z2y_NxsFw5Y2bwzpanOT0DZgiS2EF8plRQ3SsNhc6pLMtnGaNc3x9SK0Jx-heBfuL2rthkXKwKE4kglLEaxKFh_mt7pxoxuwvT4HkZQgwN-JnNZYiSaRAHsQGLAdEWy-lAXjxfFUe-j6aHug3TtxI_Cge96VJq-OZCJO3dVb3wqB5Yqa225B0Ba9XNN4B-q5Igrz1RtSGq3v8AMDOBu5knBLLz3umDWDWPHoZpTWEHG-5Tmy4-GW2CkahD4jNcDeC-TYal69Wu8Fq8QwkKE_CbtLoHz3_-A6fTnntLMzqZCvutJf_eukfjUVocCLQTfSPeAJKMFX0cF5kWIIpAeK5vtVQk4PfhzNU2SrFZJgp48XmqWGgpp6G6x7UIqlbUYzm-NuYrNIpnFSGUfhdHJYNKwdn1AQbn67OpJPq7qIAvvfPP4TSux46Gi0WcYgAon0ZhkCQVLlqPjQAFY6Mqvbw; expires=Fri, 03 Jan 2020 05:20:29 GMT; path=/; secure; samesite=lax; httponly
Transfer-Encoding: chunked

{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgxOTIyOSwiZXhwIjoxNTc2ODE5Mjg5LCJpYXQiOjE1NzY4MTkyMjl9.hbnmqwgKX81O257WsevBz1Nqxe9r_kfv4OE2byd4H7o",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgxOTIyOSwiZXhwIjoxNTc2ODYyNDI5LCJpYXQiOjE1NzY4MTkyMjl9.vIXlAOJgHaPjug4pk6lMwjq8myaFWfA-TJT9VrXVFQo"
}
```


## リフレッシュ

例：

```
$ http -v --verify=no POST https://localhost:5001/api/authentication/refresh refreshtoken=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgxOTIyOSwiZXhwIjoxNTc2ODYyNDI5LCJpYXQiOjE1NzY4MTkyMjl9.vIXlAOJgHaPjug4pk6lMwjq8myaFWfA-TJT9VrXVFQo
POST /api/authentication/refresh HTTP/1.1
Accept: application/json, */*
Accept-Encoding: gzip, deflate
Connection: keep-alive
Content-Length: 241
Content-Type: application/json
Host: localhost:5001
User-Agent: HTTPie/1.0.3

{
    "refreshtoken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgxOTIyOSwiZXhwIjoxNTc2ODYyNDI5LCJpYXQiOjE1NzY4MTkyMjl9.vIXlAOJgHaPjug4pk6lMwjq8myaFWfA-TJT9VrXVFQo"
}

HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: Fri, 20 Dec 2019 05:25:12 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgxOTUxMiwiZXhwIjoxNTc2ODE5NTcyLCJpYXQiOjE1NzY4MTk1MTJ9.ZH44O3e9kxjI05xFbgDqiyTv72JLPLUsWIviO3zr6eI"
}
```
