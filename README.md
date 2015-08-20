# ImageViewer
くるるとかそのへん用

----

- master [![Build status](https://ci.appveyor.com/api/projects/status/2nugne62ubbuknux/branch/master?svg=true)](https://ci.appveyor.com/project/mohemohe/imageviewer/branch/master)

- develop [![Build status](https://ci.appveyor.com/api/projects/status/2nugne62ubbuknux/branch/develop?svg=true)](https://ci.appveyor.com/project/mohemohe/imageviewer/branch/develop)

----

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
| Ctrl+S | Google画像検索で類似画像をさがす |

#### マウス

|                  |               機能               |
|------------------|----------------------------------|
| ドラッグ         | 画像の移動                       |
| ホイール回転     | 画像のズーム                     |
| ホイールクリック | 移動・ズームを元に戻す           |
