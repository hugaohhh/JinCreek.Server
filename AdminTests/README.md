# AdminTests.csproj

Admin.csprojのテストプロジェクト


## テストのしかた

View > Test Explorer (Ctrl+E, T) で 適当なプロジェクトを選択して実行する。  
https://docs.microsoft.com/ja-jp/visualstudio/test/getting-started-with-unit-testing?view=vs-2019#run-unit-tests  
https://docs.microsoft.com/ja-jp/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2019


## その他

- コードからのテストスタブの作成は.NET Coreではできない：  
https://docs.microsoft.com/ja-jp/visualstudio/test/unit-test-basics?view=vs-2019#generate-unit-test-project-and-unit-test-stubs
- Moq (https://www.nuget.org/packages/Moq/) を使っている
- [Authorize]属性のテスト方法  
https://stackoverflow.com/questions/48562403/unit-testing-an-authorizeattribute-on-an-asp-net-core-mvc-api-controller  
https://docs.microsoft.com/ja-jp/aspnet/core/test/integration-tests?view=aspnetcore-3.1

