# dvf-blockchain C++ using [crow](https://github.com/ipkn/crow)

[Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)をC++で写経した。

## Description

はじめにC++で書いたもの。  
VS2019を使った。ポータブルに書いたつもりだけどほかの環境では確認していない。  
パッケージマネージャを使わずに同梱可能なライセンスで規模が小さめでポータブルなライブラリをプロジェクトに含めるようにした。ただし[Boost](https://www.boost.org/)だけは別途必要。  

Webアプリケーションフレームワークは[crow](https://github.com/ipkn/crow)を使用。  
HTTPクライアントにははじめ[Boost.Beast](https://www.boost.org/doc/libs/1_70_0/libs/beast/doc/html/index.html)を使ってみたけど、単純なGETリクエストをするだけなのにややこしい感じがしたので[HappyHTTP](https://github.com/mingodad/HappyHTTP)に替えた。  
SHA256ハッシュの生成には[PicoSHA2](https://github.com/okdshin/PicoSHA2)を使ったがこれはSSEを使っていない（そのためプルーフオブワーク処理が遅い）。SSEを使っているライブラリとしては[SSE optimized SHA-256 algorithm](https://github.com/wereHamster/sha256-sse)が小さくてよさそうだけどライセンスが不明、[Sodium](https://github.com/jedisct1/libsodium)は同梱するには大きすぎる。  

Visual Studio 2019からMicrosoft.Cpp.Win32.userプロパティシートがなくなったため、Blockchain.propsを作成してBoostという名前でBoostライブラリのパスを指定すること（[サンプル](https://github.com/teheperor/dvf-blockchain/blob/master/sample/Blockchain.props)）。  

## Dependency

 - [Boost](https://www.boost.org/doc/libs/1_70_0/)
 - [JSON for Modern C++](https://github.com/nlohmann/json)
 - [PicoSHA2](https://github.com/okdshin/PicoSHA2)
 - [url-cpp](https://github.com/seomoz/url-cpp)
 - [crow](https://github.com/ipkn/crow)
 - [HappyHTTP](https://github.com/mingodad/HappyHTTP)
 - [cmdline](https://github.com/tanakh/cmdline)
 - [Catch2](https://github.com/catchorg/Catch2)

## License

[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

## References
- [Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)
- [dvf/blockchain: A simple Blockchain in Python](https://github.com/dvf/blockchain)
- [ブロックチェ－ンを構築しながら学ぶ \| POSTD](https://postd.cc/learn-blockchains-by-building-one/)
