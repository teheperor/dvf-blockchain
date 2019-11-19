# dvf-blockchain

[Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)をPython以外のプログラミング言語で写経した。  

## Description

はじめにC#で写経してみたら、コード量が少ないわりにHTTPアプリケーションフレームワーク＆クライアント、JSON、ハッシュ生成とかいろいろな要素が入っていてプログラミング言語や開発環境やエコシステムの学習のいい題材になると思った。  
それでほかのプログラミング言語でもやってみることにしてC#、C++、JavaScript、Go、TypeScript、Rust、Kotlin、F#で写経した。  

基本的な方針は[オリジナルのPythonのコード](https://github.com/dvf/blockchain)の設計、構造、名前、エラー処理、処理結果をできるだけ維持して写経すること。  
処理結果、特にプルーフオブワーク処理の結果については異なるプログラミング言語による実装間での互換性は維持しない。理由はタイムスタンプの精度（桁数）やJSONのフォーマットを合わせるのが面倒くさいから。  
もともとのコードがブロックチェーンのエッセンスの説明のためのものでエラー処理は雑であり写経でも同程度にした。  
それぞれのプログラミング言語や開発環境やエコシステムの学習を重視する。  

- C# [blockchain-netfx](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-netfx)
  - はじめにC#で書いた。
- C# [blockchain-netcore](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-netcore)
  - [blockchain-netfx](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-netfx)を.NET Coreのプロジェクトにして更新した。
- C++ [blockchain-cpp-crow](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp-crow)
  - はじめにC++で書いたもの。
- C++ [blockchain-cpp](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp)
  - [blockchain-cpp-crow](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp-crow)で使用している[ipkn/crow](https://github.com/ipkn/crow)の開発が止まっていることと、ほかのものも試してみたかったのでWebアプリケーションフレームワークを[cpp-httplib](https://github.com/yhirose/cpp-httplib)に変更した。
- JavaScript(Node.js) [blockchain-js](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-js)
- Go [blockchain-go](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-go)
- TypeScript [blockchain-ts](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-ts)
- Rust [blockchain-rust](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-rust)
- Kotlin [blockchain-kotlin](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-kotlin)
- F# [blockchain-fsharp](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-fsharp)

## Test

[postman](https://www.getpostman.com/products)の[テスト・コレクション](https://github.com/teheperor/dvf-blockchain/tree/master/postman)とPythonの[テスト](https://github.com/teheperor/dvf-blockchain/tree/master/test)を書いた。  
どちらも同じ内容で2つのノードにアクセスして正常系の処理だけを一通りテストする。  

## License

[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

## References
- [Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)
- [dvf/blockchain: A simple Blockchain in Python](https://github.com/dvf/blockchain)
- [ブロックチェ－ンを構築しながら学ぶ \| POSTD](https://postd.cc/learn-blockchains-by-building-one/)
