# まのさばAIキャラと人狼できるゲーム　プロトタイプ

## 免責事項
- 本作はADVゲーム[魔法少女ノ魔女裁判](https://manosaba.com/)の非公式二次創作ゲームです。
- このソフトウェアには生成AIの利用が含まれています。
- キャラクターのアニメーションに[EmoteLab](https://emotelab.app/)を利用しています。
- 完全に技術デモみたいなもので、**バグの多さ**や**使いづらさ**が山積しています。
- **ある程度PC慣れしている方に構築を推奨致します。**
- **Windows x64でのみ動作確認しています。**ほかのプラットフォームでの動作は保証しません。
- Xで以前公開していたキャラアニメーションは、[作者様](https://x.com/gi___to)の規約により別のものに差し替えています。必要に応じてカスタムすることも可能です。（後述）
- このソフトウェアをダウンロード・実行したことによる責任を、開発者は一切負いません。

## 遊び方
### 1. ダウンロード
1. [wolfgame.avalonia](https://github.com/ok23902/wolfgame.avalonia/releases/download/v1.0.2/wolfgame.avalonia_v1.0.2.zip)をダウンロードします。
2. zipファイルを展開してください。

### 2. APIキー取得
AIの利用にAPIキーというものが必要です。今回は無料で使える**Google AI Studio**で設定を行います。
送信した情報はGoogleのサービス改善に利用される可能性があるので注意してください。
1. [Google AI Studio](https://aistudio.google.com/api-keys)にアクセスします。

2. [Google AI Studio](https://aistudio.google.com/api-keys)にアクセスし、Googleでログインしてください。
3. 下画像のような画面が出ると思うので、「**APIキーを作成**」ボタンをクリックしてください。
<div>
<img width="1593" height="1027" alt="API Key 1" src="https://github.com/user-attachments/assets/6b2be415-4153-463c-a263-b84aae98582a" />
</div>

4. 「キー名の設定」や「インポートしたプロジェクトを選択」は何でもいいので、「**キーを作成**」をクリックしてください。
<div>
<img width="630" height="367" alt="API Key 2" src="https://github.com/user-attachments/assets/144e15ce-ee04-4337-84d7-8428e4718fb4" />
</div>

5. 「**鍵をコピー**」してAPIキーをコピーしてください。
<div>
<img width="654" height="648" alt="API KEY 3" src="https://github.com/user-attachments/assets/83039b56-4063-4a2c-bcb1-31c5255eb693" />
</div>

### 3. APIキーの設定
1. 解凍したフォルダの「`wolfgame.avalonia_v1.0.2\Config\env`」という場所に「`.env`」ファイルがあることを確認してください。
3. 任意のテキストエディターで「`.env`」を開き、「`MANO_LLM_API_KEY=`」の「`=`」の後に、先ほど取得した**APIキー**を貼り付けてください。
<div>
<img width="1126" height="106" alt="API KEY Set" src="https://github.com/user-attachments/assets/bd23c055-b06b-42cf-a18b-bd1e8713327b" />
</div>

4. `.env`を保存したことを確認してください。

### 4. 起動
1. 回答したフォルダ直下にある「`WolfGame.Avalonia.exe`」を起動してください。
2. 「**ゲーム開始**」でゲームが開始できます。右側の入力欄でチャットすることができます。

## 補足
### キャラのカスタマイズ
1. `wolfgame.avalonia_v1.0.2\Config\data`に、`character.json`ファイルがあるのを確認してください。
2. これらに記述されている`Name`、`Personality`、`Goal`を書き換えることでキャラの名前や性格を変更することができます。
3. `type`に関して、プレイヤーの場合は「`PlayerObject`」、AIキャラの場合は「`CharacterAIObject`」と入力してください。何体追加しても構いませんが、APIの消費量が増えるので注意してください。

### キャラ画像・アニメーションのカスタマイズ
1. `wolfgame.avalonia_v1.0.2\Config\sprites`に、**各キャラの`Id`にあったフォルダが作成されているのを確認してください。**
2. キャラを差し替えたい場合は、例のように「{Id}_{エモート名}.gif」にリネームして差し替えてください。
- **`Id`が一致しない場合反映されないので注意してください。**
- **`.gif`のみの対応なのでご了承ください。**
- **現状例にあるエモート名しかAIは認識しません。**その範囲でお願いします。

### ご自身のAPIキーへの差し替え
ChatGPTやClaude、GeminiなどOpenAIの規格に合ったAPIキーを既に所有している方に関してです。
1. `wolfgame.avalonia_v1.0.2\Config\env`に、`.env`ファイルがあるのを確認してください。
2. それぞれAPIキー、モデル名、エンドポイントを入力することで差し替えることができます。
- モデル名やエンドポイントはサービスによって異なるので、個別に調べる必要があります。
- 認識しなかった場合、画面に何も表示されなくなります。
- プロトタイプなのでバグによるAPIの大量リクエストが起こる可能性がないとは言い切れません。くれぐれも自己責任でお願いします。

### 参考
- 原作：[魔法少女ノ魔女裁判](https://manosaba.com/)。
- キャラの性格設定を[ピクシブ百科事典](https://dic.pixiv.net/)に参考にさせていただいています。
- コードの一部にAI生成があります。
- **アニメーションGifの生成**に[EmoteLab](https://emotelab.app/)というツールを使用しています。**キャラを差し替える際に参考になるかもしれません。**

## 余談
- 既に別のアプローチのプロジェクトを進めているので、更新はないかもしれません。あくまでプロトタイプです。