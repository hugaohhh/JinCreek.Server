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


## 3 トークンの取得

`/api/authentication/login`に`organizationCode`と`domainName`と`userName`と`password`をPOSTすると`accessToken`と`refreshToken`が返る。

例：

```
$ http -v POST http://localhost:5000/api/authentication/login organizationCode:=1 domainName="domain01" userName="user1" password="user1"
POST /api/authentication/login HTTP/1.1
Accept: application/json, */*
Accept-Encoding: gzip, deflate
Connection: keep-alive
Content-Length: 91
Content-Type: application/json
Host: localhost:5000
User-Agent: HTTPie/1.0.3

{
    "domainName": "domain01",
    "organizationCode": 1,
    "password": "user1",
    "userName": "user1"
}

HTTP/1.1 200 OK
Content-Length: 530
Content-Type: application/json; charset=utf-8
Date: Mon, 17 Feb 2020 10:03:46 GMT
Server: Kestrel

{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImViNzUwNDk3LWYzMjUtNDY5MS05NDI1LWJiZTdkM2ZhYmNjMCIsInJvbGUiOiJVc2VyQWRtaW4iLCJuYmYiOjE1ODE5MzM4MjcsImV4cCI6MTU4MTkzMzg4NywiaWF0IjoxNTgxOTMzODI3fQ.lo7Zq-Dih2JigVH3EtEl7sFC_Z3cLsI5vxOUv5s4AEk",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImViNzUwNDk3LWYzMjUtNDY5MS05NDI1LWJiZTdkM2ZhYmNjMCIsInJvbGUiOiJVc2VyQWRtaW4iLCJuYmYiOjE1ODE5MzM4MjcsImV4cCI6MTU4MTk3NzAyNywiaWF0IjoxNTgxOTMzODI3fQ.qOWz4gvDbUWmoKNKFKzoiY8Eom3dEZKDip2HBE1OCzo"
}
```

スーパー管理者の場合は`organizationCode`と`domainName`をつけない。

例：

```
$ http -v POST http://localhost:5000/api/authentication/login username="user0" password="user0"
POST /api/authentication/login HTTP/1.1
Accept: application/json, */*
Accept-Encoding: gzip, deflate
Connection: keep-alive
Content-Length: 42
Content-Type: application/json
Host: localhost:5000
User-Agent: HTTPie/1.0.3

{
    "password": "user0",
    "username": "user0"
}

HTTP/1.1 200 OK
Content-Length: 532
Content-Type: application/json; charset=utf-8
Date: Mon, 17 Feb 2020 10:01:37 GMT
Server: Kestrel

{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Ijk4Mjg4MWRiLTFlMWQtNDZmMy1hM2UxLTkwOGM5Zjc2NWVlNCIsInJvbGUiOiJTdXBlckFkbWluIiwibmJmIjoxNTgxOTMzNjk3LCJleHAiOjE1ODE5MzM3NTcsImlhdCI6MTU4MTkzMzY5N30.e9XnuMBMkYQFQIMAE1OzyddJ7mB0z6qnKqlQqUTLF1k",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Ijk4Mjg4MWRiLTFlMWQtNDZmMy1hM2UxLTkwOGM5Zjc2NWVlNCIsInJvbGUiOiJTdXBlckFkbWluIiwibmJmIjoxNTgxOTMzNjk3LCJleHAiOjE1ODE5NzY4OTcsImlhdCI6MTU4MTkzMzY5N30.oYxo0g3tfATdls8EHKgEijXslMEJw3LTt8h16lhbo2w"
}
```


## 4 アクセス

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


## 5 リフレッシュ

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


## 6 おまけ

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


## 7 おまけ2

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
