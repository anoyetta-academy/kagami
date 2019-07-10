# kagami

「カガミ」はスキル回しを見やすく表示するためのツールです。OverlayPlugin の Add-on として動作します。  

* 配信のときに視聴者にスキル回しを見せることが出来る
* 自分で録画してスキル回しをチェックすることが出来る
* 木人でのスキル回し動画を作成するときにスキル回しを見せやすくなる

![preview](https://github.com/ramram1048/kagamireact/raw/master/mdimages/preview.gif)

### **[DOWNLOAD](<https://github.com/anoyetta-academy/kagami/releases>)**

## インストール
1. ダウンロードする
2. ダウンロードしたものを解凍する
3. ダウンロードしたものをOverlayPluginのインストールディレクトリに上書きコピーする
   ```
   OverlayPlugin
    ├ addons
    │  └ kagami.dll (new)
    └ resources
       └ kagami (new)
   ```

## 使い方
1. OverlayPlugin の設定タブで [New] をクリックする
2. オーバーレイの Name を入力する。例えば、"KAGAMI" とする
3. オーバーレイの Type で kagami を選択する
4. OK をクリックする
5. kagami オーバーレイの設定が表示されたら URL に使用したいスキンの URL を入力する

## スキン
### ram（発案者） さんのスキン
* レポジトリ  
https://github.com/ramram1048/kagamireact  
* スキンURL  
```https://ramram1048.github.io/kagamireact/```

## 制限事項
* ゴーストモードは未実装です

## FAQ
* オーバーレイがOBSの配信に映らない  
Chrome のハードウェアアクセラレータを切りましょう。  
OverlayPlugin は Chrome を使用して描画しているため一部の設定は Chrome 本体の設定の影響を受けます。 
下記の手順を実行してください。  
    1. Chrome のメニューを開く
    2. [設定] をクリックする
    3. [詳細設定] をクリックする
    4. [ハードウェア アクセラレーションが使用可能な場合は使用する] をOFFにする
    5. 念のため PC を再起動する
