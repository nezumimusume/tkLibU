# tkLibU
tkLibUは河原電子ビジネス専門学校での使用を目的に開発しているUnity向けのライブラリです。</br>
今後機能を追加予定です。

## 1. tkVlit(tk Volume Light)
ボリュームライト。現在はスポットライトのボリュームライトをサポートしています。
### 1.1 アルゴリズム概要

描画アルゴリズムは下記のようになっています。
1. スポットライトモデルの背面の深度値を描画
2. スポットライトモデルの表面の深度値を描画
3. 1と2で作成された深度情報を元にボリュームライトを描画。

アルゴリズムの詳細は下記ページを参照してください。</br>
http://maverickproj.web.fc2.com/pg44.html

### 1.2 実装概要
CommandBufferを利用して実装されており、シーンをレンダリングしているカメラの半透明描画(フォワードレンダリングのパス)が実行される直前にボリュームライトを描画する描画コマンドが実行されている。CommandBufferを構築している処理はtkVlit_DrawVolumeLight::OnPreRender()関数に記載されている。

### 1.2 使用方法
下記動画を参照</br>
https://youtu.be/Y8Wgt_oHqcM

### 1.3 動作確認環境
下記の環境で動作確認済み</br>
**レンダリングパイプライン**</br>
　ビルトインパイプライン(フォワード、ディファ―ド)、URP</br>
**プラットフォーム**</br>
　Windows( GTX1060,RTX3070 )</br>
　Android( Pixel 6 Pro, Pixel 3, Xperia SO-03J )</br>
　Mac( M1 macbook pro )</br>
　iOS( iPad第6世代 )</br>

