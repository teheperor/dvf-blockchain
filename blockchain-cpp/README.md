# dvf-blockchain C++

[Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)をC++で写経した。

## Description

Web APIフレームワークを[crow](https://github.com/ipkn/crow)から[cpp-httplib](https://github.com/yhirose/cpp-httplib)を使うように切り替えて書いたのがこれ。
その他の変更点:
 - httpクライアントな処理を[HappyHTTP](https://github.com/mingodad/HappyHTTP)や[Boost.Beast](https://www.boost.org/doc/libs/1_70_0/libs/beast/doc/html/index.html)ではなく[cpp-httplib](https://github.com/yhirose/cpp-httplib)を使うように変えた。
 - [crow](https://github.com/ipkn/crow)と違って[cpp-httplib](https://github.com/yhirose/cpp-httplib)はデフォルトでアクセスログを出力してくれなかったので[easyloggingpp](https://github.com/zuhd-org/easyloggingpp)でログを出力するようにした。
 - ユニットテスト・フレームワークを無駄に[Catch2](https://github.com/catchorg/Catch2)から[doctest](https://github.com/onqtam/doctest)に変えてみた。

## Dependency

 - [Boost](https://www.boost.org/doc/libs/1_70_0/)
 - [JSON for Modern C++](https://github.com/nlohmann/json)
 - [PicoSHA2](https://github.com/okdshin/PicoSHA2)
 - [url-cpp](https://github.com/seomoz/url-cpp)
 - [cpp-httplib](https://github.com/yhirose/cpp-httplib)
 - [easyloggingpp](https://github.com/zuhd-org/easyloggingpp)
 - [cmdline](https://github.com/tanakh/cmdline)
 - [doctest](https://github.com/onqtam/doctest)

## License

[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

## References
- [Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)
- [dvf/blockchain: A simple Blockchain in Python](https://github.com/dvf/blockchain)
- [ブロックチェ－ンを構築しながら学ぶ \| POSTD](https://postd.cc/learn-blockchains-by-building-one/)
