# dvf-blockchain

[Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)をオリジナルのPython以外のプログラミング言語で写経した。

## Description

はじめにC#で写経してみたら、コード量が少ないわりにHTTPクライアント＆サーバー、JSON、ハッシュ生成とかいろいろな要素が入っていてプログラミング学習のいい題材になると思った。  
それでほかのプログラミング言語でもやってみることにして、今までC#、C++、JavaScript、Go、TypeScript、Rust、Kotlin、F#で写経した。

- [blockchain-netfx](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-netfx)
  - はじめにC#で写経した。
- [blockchain-netcore](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-netcore)
  - .NET Coreでは[Self Hosting Nancy](https://github.com/NancyFx/Nancy/wiki/Self-Hosting-Nancy)が使えなかったため[ASP\.NET CoreのKestrel Webサーバー](https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/servers/kestrel)に替えた。
- [blockchain-cpp-crow](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp-crow)
  - はじめにC++で写経した。
  - [crow](https://github.com/ipkn/crow)はずいぶん更新されていないので[cpp\-httplib](https://github.com/yhirose/cpp-httplib)に切り替えて[blockchain-cpp](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp)を書いた。
- [blockchain-cpp](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp)
  - C++版を[crow](https://github.com/ipkn/crow)から[cpp\-httplib](https://github.com/yhirose/cpp-httplib)を使うように切り替えた。
- [blockchain-js](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-js)
  - JavaScript(Node.js)で写経。
  - プローフオブワーク処理が重いのでworker_threadsモジュールを使ったバージョンも追加した。
- [blockchain-go](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-go)
  - Goで写経。
- [blockchain-ts](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-ts)
  - [blockchain-js](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-js)をTypeScriptで書き換えた。
- [blockchain-rust](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-rust)
  - Rustで写経。
- [blockchain-kotlin](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-kotlin)
  - Kotlinで写経。
- [blockchain-fsharp](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-fsharp)
  - 関数型言語をやってみたかったのでF#で写経。

## Test

[postman](https://github.com/teheperor/dvf-blockchain/tree/master/postman)に[postman](https://www.getpostman.com/products)でのテスト・コレクション、[test](https://github.com/teheperor/dvf-blockchain/tree/master/test)にpython3でのテスト・スクリプトがある。  
どちらも次のWeb APIにアクセスして応答をテストをする。  
（Server1とServer2のデフォルト値はそれぞれhttp://localhost:5000とhttp://localhost:5001）
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
