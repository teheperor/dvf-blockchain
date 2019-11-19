# dvf-blockchain C#(.NET Core)

[Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)をC#(.NET Core)で写経した。

## Description

[blockchain-netfx](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-netfx)を.NET Coreのプロジェクトにしたもの。  
.NET Coreでは[Self Hosting Nancy](https://github.com/NancyFx/Nancy/wiki/Self-Hosting-Nancy)が使えなかったため[ASP.NET CoreのKestrel Webサーバー](https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/servers/kestrel)に替えた。  
[.NET Core 3.0でC# 8.0になった](https://docs.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-core-3-0#c-80)ので[null許容参照型](https://docs.microsoft.com/ja-jp/dotnet/csharp/nullable-references)を導入した。  

## Dependency
 - [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)
 - [Nancy](https://github.com/NancyFx/Nancy)
 - [NLog](https://github.com/NLog/NLog)
 - [CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils)
 - [xUnit.net](https://github.com/xunit/xunit)
 - [FluentAssertions](https://github.com/fluentassertions/fluentassertions)

## License

[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

## References
- [Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)
- [dvf/blockchain: A simple Blockchain in Python](https://github.com/dvf/blockchain)
- [ブロックチェ－ンを構築しながら学ぶ \| POSTD](https://postd.cc/learn-blockchains-by-building-one/)
