README
======

## 必要

* Visual Studio 2017
* NodeJS

## 開発するとき

1. MyCMS\MyCMS.sln を VisualStudioで起動してF5で実行する。
2. MyCMS.Front をコマンドプロンプトやPowerShellで開き、npm startを実行する。

## プロダクションで使うとき

1. MyCMS.Front をコマンドプロンプトやPowerShellで開き、npm run buildprodを実行する。
2. MyCMSのwwwrootを削除して、MyCMS.Frontのwwwrootをコピーする。