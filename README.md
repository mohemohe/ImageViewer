# ImageViewer
くるるとかそのへん用

## ダウンロード

- 常用版（≠安定版）  
[![Build status](https://ci.appveyor.com/api/projects/status/2nugne62ubbuknux/branch/master?svg=true)](https://ci.appveyor.com/project/mohemohe/imageviewer/branch/master/artifacts)  

- 開発版  
[![Build status](https://ci.appveyor.com/api/projects/status/2nugne62ubbuknux/branch/develop?svg=true)](https://ci.appveyor.com/project/mohemohe/imageviewer/branch/develop/artifacts)

---

## もくじ

- つかいかた
- そうさほうほう
- こんなときは
- らいぶらり

## つかいかた

第一引数のURLが画像の場合は内蔵ビューアで表示します。  
そうでない場合は既定のブラウザで開きます。

`\path\to\ImageVIewer.exe %1`

![](http://i.imgur.com/YG5QAn5.png)

## そうさほうほう

一般的な画像ビューアに近づけていますが、実装していない操作も多いです。

#### キーボード

下部のボタン・右クリックメニューでも同様の操作ができます。

|        |               機能               |
|--------|----------------------------------|
| Ctrl+S | 名前をつけて保存                 |
| Ctrl+C | クリップボードにコピー           |
| Ctrl+O | ブラウザで開く                   |
| Ctrl+F | Google画像検索で類似画像をさがす |

#### マウス

- タブ

|                  |               機能               |
|------------------|----------------------------------|
| ホイール回転     | タブの移動                       |
| ホイールクリック | タブを閉じる                     |

- 表示領域

|                  |               機能               |
|------------------|----------------------------------|
| ドラッグ         | 画像の移動                       |
| ホイール回転     | 画像のズーム                     |
| ホイールクリック | 移動・ズームを元に戻す           |

## こんなときは

#### 起動が遅い、なんかもっさりする

.NET Frameworkのネイティブイメージが生成されていないのが原因です。  
通常、ネイティブイメージはPCがアイドル状態のときに自動生成されますが、.NET Frameworkの更新などにより、再度生成しなければならない場合があります。  
管理者権限のPowerShellで

    Get-ChildItem "C:\Windows\Microsoft.NET\Framework64\v4.0.30319" -Filter *.dll | Foreach-Object{ C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ngen.exe install $_.FullName }

するとネイティブアプリケーションに近い速度で動作するようになります。  
（※ 32bit版のWindowsでは Framework64 を Framework に読み替えてください。）

#### 動作がおかしい、○○のアップローダーにも対応してほしい

[issue](https://github.com/mohemohe/ImageViewer/issues) を立ててください。  
対応を保証するものではありませんが、可能なものは応えます。

## らいぶらり

- [Livet](https://github.com/ugaya40/Livet)
- [QuickConverter](https://quickconverter.codeplex.com/)