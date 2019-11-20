# dvf-blockchain C++

[Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)をC++で写経した。

## Description

[blockchain-cpp-crow](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-cpp-crow)で使用している[ipkn/crow](https://github.com/ipkn/crow)の開発が止まっていることと、ほかのものも試してみたかったのでWebアプリケーションフレームワークを[cpp-httplib](https://github.com/yhirose/cpp-httplib)に変更した。  
[Boost](https://www.boost.org/)への依存をなくして、UUID V4生成には[sole](https://github.com/r-lyeh-archived/sole)を使うようにした。  

その他の変更点:
 - HTTPクライアントを[HappyHTTP](https://github.com/mingodad/HappyHTTP)や[Boost.Beast](https://www.boost.org/doc/libs/1_70_0/libs/beast/doc/html/index.html)から[cpp-httplib](https://github.com/yhirose/cpp-httplib)に替えた。
 - [cpp-httplib](https://github.com/yhirose/cpp-httplib)はデフォルトでアクセスログを出力してくれなかったので[easyloggingpp](https://github.com/zuhd-org/easyloggingpp)でログを出力するようにした。
 - ユニットテスト・フレームワークを無駄に[Catch2](https://github.com/catchorg/Catch2)から[doctest](https://github.com/onqtam/doctest)に変えてみた。

## Dependency

 - [Boost](https://www.boost.org/doc/libs/1_70_0/)
 - [JSON for Modern C++](https://github.com/nlohmann/json)
 - [PicoSHA2](https://github.com/okdshin/PicoSHA2)
 - [url-cpp](https://github.com/seomoz/url-cpp)
 - [cpp-httplib](https://github.com/yhirose/cpp-httplib)
 - [easyloggingpp](https://github.com/zuhd-org/easyloggingpp)
 - [cmdline](https://github.com/tanakh/cmdline)
 - [sole](https://github.com/r-lyeh-archived/sole)
 - [doctest](https://github.com/onqtam/doctest)

## License

[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

## References
- [Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)
- [dvf/blockchain: A simple Blockchain in Python](https://github.com/dvf/blockchain)
- [ブロックチェ－ンを構築しながら学ぶ \| POSTD](https://postd.cc/learn-blockchains-by-building-one/)
