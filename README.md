# tkLibU
tkLibUは河原電子ビジネス専門学校での使用を目的に開発している、Unity向けのライブラリです。</br>
今後機能を追加予定です。

## 1. tkVlit(tk Volume Light)
ボリュームライト。現在はスポットライトのボリュームライトをサポートしています。
### アルゴリズム概要

描画アルゴリズムは下記のようになっています。
1. スポットライトモデルの背面の深度値を描画
2. スポットライトモデルの表面の深度値を描画
3. 1と2で作成された深度情報を元にボリュームライトを描画。

アルゴリズムの詳細は下記ページを参照してください。</br>
http://maverickproj.web.fc2.com/pg44.html

### 使用方法
1. シーンをレンダリングしているメインカメラにtkVlit_DrawVolumeLightコンポーネントを追加(Component/tkLibU/tkVlit/tkVlit_DrawVolumeLight)
2. シーンにスポットライトのゲームオブジェクトを追加(GameObject/tkLibU/tkVlit/tkVlit_SpotLight)

操作方法は下記動画を参照</br>

### 動作確認環境
Unityのデフォルトレンダリングパイプラインのフォワードレンダリングでのみ動作確認済み(Windows)。
Mac、URP、ディファードレンダリング、iOS、Androidではまだ動作未確認。
今後動作確認後、対応予定。
