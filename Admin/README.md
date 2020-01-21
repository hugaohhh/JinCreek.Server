# Admin.csproj

管理画面 (Angular) が呼び出すWeb API


## 1 マイグレーション

Package Manager Console (View > Other Windows > Package Manager Console) で：

```
PM> Update-Database -Project Admin -StartupProject Admin
```

see https://docs.microsoft.com/ja-jp/ef/core/miscellaneous/cli/powershell#common-parameters


## 2 Swagger

以下でSwaggerが見られる（NSwagを使用）：

- http://localhost:5000/swagger/
- https://localhost:5001/swagger/
- http://localhost:5000/swagger/v1/swagger.json
- https://localhost:5001/swagger/v1/swagger.json


## 3 ユーザ登録（テスト用）

`/api/authentication/register`に`username`と`password`をJSON形式(`Content-Type: application/json`)でPOSTするとユーザが作成される。
`password`は6文字以上で、英字大文字と英字小文字と数字と記号を含む必要がある。

例：

```
$ http -v POST http://localhost:5000/api/authentication/register username="t-suzuki@indigo.co.jp" password="9Q'vl!"
POST /api/authentication/register HTTP/1.1
Accept: application/json, */*
Accept-Encoding: gzip, deflate
Connection: keep-alive
Content-Length: 59
Content-Type: application/json
Host: localhost:5000
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


## 4 トークンの取得

`/api/authentication/login`に`username`と`password`をPOSTすると`accessToken`と`refreshToken`が返る。

例：

```
$ http -v POST http://localhost:5000/api/authentication/login username="t-suzuki@indigo.co.jp" password="9Q'vl!"
POST /api/authentication/login HTTP/1.1
Accept: application/json, */*
Accept-Encoding: gzip, deflate
Connection: keep-alive
Content-Length: 59
Content-Type: application/json
Host: localhost:5000
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


## 5 アクセス

`accessToken`を`Authorization: Bearer`ヘッダにつけて`GET`する。

例：

```
$ http -v GET http://localhost:5000/api/randomnumber/generate Authorization:"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgzMjU3MSwiZXhwIjoxNTc2ODMyNjMxLCJpYXQiOjE1NzY4MzI1NzF9.A1MF2O82jDtB9yp3_fVVrJ2Uv5q4IDZlAhhw4EvHQ3Y"
GET /api/randomnumber/generate HTTP/1.1
Accept: */*
Accept-Encoding: gzip, deflate
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgzMjU3MSwiZXhwIjoxNTc2ODMyNjMxLCJpYXQiOjE1NzY4MzI1NzF9.A1MF2O82jDtB9yp3_fVVrJ2Uv5q4IDZlAhhw4EvHQ3Y
Connection: keep-alive
Host: localhost:5000
User-Agent: HTTPie/1.0.3



HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: Fri, 20 Dec 2019 09:03:49 GMT
Server: Kestrel
Transfer-Encoding: chunked

665
```


## 6 リフレッシュ

`/api/authentication/refresh`に`refreshtoken`をPOSTすると、新しい`accessToken`が返る。

例：

```
$ http -v POST http://localhost:5000/api/authentication/refresh refreshtoken=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgxOTIyOSwiZXhwIjoxNTc2ODYyNDI5LCJpYXQiOjE1NzY4MTkyMjl9.vIXlAOJgHaPjug4pk6lMwjq8myaFWfA-TJT9VrXVFQo
POST /api/authentication/refresh HTTP/1.1
Accept: application/json, */*
Accept-Encoding: gzip, deflate
Connection: keep-alive
Content-Length: 241
Content-Type: application/json
Host: localhost:5000
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


## 7 おまけ

accessTokenのペイロード：

```
"accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgxOTIyOSwiZXhwIjoxNTc2ODE5Mjg5LCJpYXQiOjE1NzY4MTkyMjl9.hbnmqwgKX81O257WsevBz1Nqxe9r_kfv4OE2byd4H7o",
```

↓

```
{
  "unique_name": "369b9e52-cdf2-4a85-bf3b-dd05b1fd6cb6",
  "nbf": 1576819229,
  "exp": 1576819289,
  "iat": 1576819229
}
```

トークンタイプが含まれていない!


refreshTokenのペイロード：

```
"refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgxOTIyOSwiZXhwIjoxNTc2ODYyNDI5LCJpYXQiOjE1NzY4MTkyMjl9.vIXlAOJgHaPjug4pk6lMwjq8myaFWfA-TJT9VrXVFQo"
```

↓

```
{
  "unique_name": "369b9e52-cdf2-4a85-bf3b-dd05b1fd6cb6",
  "nbf": 1576819229,
  "exp": 1576862429,
  "iat": 1576819229
}
```

トークンタイプが含まれていない!


## 8 おまけ2

エラーのときに例えば次のようなレスポンスボディが返る（see [既定の BadRequest 応答](https://docs.microsoft.com/ja-jp/aspnet/core/web-api/index?view=aspnetcore-3.1#default-badrequest-response)）：

```
{
    "errors": {
        "Password": [
            "The Password field is required."
        ],
        "UserName": [
            "The UserName field is required."
        ]
    },
    "status": 400,
    "title": "One or more validation errors occurred.",
    "traceId": "|987ed40a-43c30b9d932aa609.",
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1"
}
```

このうち`traceId`は
https://github.com/aspnet/AspNetCore/blob/release/3.1/src/Mvc/Mvc.Core/src/Infrastructure/DefaultProblemDetailsFactory.cs#L93
でセットしている。
内容は`Activity.Current.Id` ([doc](https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/ActivityUserGuide.md)) か、`Activity.Current.Id`がnullであれば`httpContext.TraceIdentifier`がセットされる。
通常は`Activity.Current.Id`がセットされる。
