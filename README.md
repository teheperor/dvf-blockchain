# dvf-blockchain

[Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)をオリジナルのPython以外のプログラミング言語で写経した。

## Description

はじめにC#で写経してみたら、HTTPのサーバーとクライアントやJSONの入出力とか、細かいところでハッシュ生成とかコマンドラインパーサーとかロガーとかが必要になったりして、これは新しく触れるプログラミング言語とその環境についてのいい学習題材になると思った。  
そんなわけでC#で書いた後にC++、JavaScript(Node.js)、Goで書いてみた。

- [blockchain-cpp-crow](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp-crow)
  - はじめにC++で写経した。
  - [crow](https://github.com/ipkn/crow)はずいぶん更新されていないので[cpp\-httplib](https://github.com/yhirose/cpp-httplib)に切り替えて[blockchain-cpp](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp)を書いた。
- [blockchain-cpp](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp)
  - C++版を[crow](https://github.com/ipkn/crow)から[cpp\-httplib](https://github.com/yhirose/cpp-httplib)を使うように切り替えた。
  - ロギング処理などを更新した。
  - 元のC++版は[blockchain\-cpp\-crow](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp-crow)
- [blockchain-netfx](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-netfx)
  - はじめにC#で写経した。
- [blockchain-netcore](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-netcore)
  - .NET Coreでは[Self Hosting Nancy · NancyFx/Nancy Wiki](https://github.com/NancyFx/Nancy/wiki/Self-Hosting-Nancy)が使えなかったため[ASP\.NET Core への Kestrel Web サーバーの実装 \| Microsoft Docs](https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-2.2)に切り替えた。
- [blockchain-js](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-js)
  - JavaScript(ES2017)で写経。
  - プローフオブワーク処理が重いのでworker_threadsモジュールを使ったバージョンも書いた。
- [blockchain-go](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-go)
  - Goで写経。

## Test

[postman](https://github.com/teheperor/dvf-blockchain/tree/master/postman)に[postman](https://www.getpostman.com/products)でのテスト・コレクション、[test](https://github.com/teheperor/dvf-blockchain/tree/master/test)にpython3でのテスト・スクリプトがある。  
どちらも次のWeb APIにアクセスして応答をテストをする（これは正常系のテストのみ）。
- Server1 /chain
- Server1 /mine
- Server1 /chain
- Server1 /transactions/new
- Server1 /mine
- Server1 /chain
- Server2 /chain
- Server2 /mine
- Server2 /chain
- Server1 /nodes/register
- Server1 /nodes/resolve
- Server2 /nodes/register
- Server2 /nodes/resolve

## License

[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

## References
- [Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)
- [dvf/blockchain: A simple Blockchain in Python](https://github.com/dvf/blockchain)
- [ブロックチェ－ンを構築しながら学ぶ \| POSTD](https://postd.cc/learn-blockchains-by-building-one/)
