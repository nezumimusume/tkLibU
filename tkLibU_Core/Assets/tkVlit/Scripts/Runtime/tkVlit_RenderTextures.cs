using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tkLibU
{
    /// <summary>
    /// ボリュームライトの描画で使うレンダリングテクスチャたち。
    /// </summary>
    public class tkVlit_RenderTextures
    {
        RenderTexture m_backFaceDepthTexture; // 背面の深度値が書き込まれているテクスチャ。
        RenderTexture m_frontFaceDepthTexture; // 表面の深度値が書き込まれているテクスチャ。
        RenderTexture m_finalTexture; // 最終描画結果の書き込み先。
        int m_depthMapWidth; // 深度マップの幅。
        int m_depthMapHeight; // 深度マップの高さ。
        bool m_isInitedRenderTexture = false; // レンダリングテクスチャが初期化済みかどうかのフラグ。

        /// <summary>
        /// 背面の深度値が書き込まれているテクスチャのプロパティ
        /// </summary>
        public RenderTexture backFaceDepthTexture
        {
            get => m_backFaceDepthTexture;
        }

        /// <summary>
        /// 前面の深度値が書き込まれているテクスチャのプロパティ。
        /// </summary>
        public RenderTexture frontFaceDepthTexture
        {
            get => m_frontFaceDepthTexture;
        }

        /// <summary>
        /// 最終結果を描画結果を書き込むテクスチャのプロパティ。
        /// </summary>
        public RenderTexture finalTexture
        {
            get => m_finalTexture;
        }

        /// <summary>
        /// 毎フレーム呼び出す更新処理
        /// </summary>
        public void Update()
        {
            if (m_depthMapWidth != Screen.width || m_depthMapHeight != Screen.height)
            {
                Init();
            }
        }

        /// <summary>
        /// 各種レンダリングテクスチャを初期化する。
        /// </summary>
        public void Init()
        {
            if (Screen.width == 0 || Screen.height == 0)
            {
                return;
            }

            // 深度テクスチャの幅高さを初期化。
            m_depthMapWidth = Screen.width;
            m_depthMapHeight = Screen.height;

            if (m_isInitedRenderTexture)
            {
                // 再初期化になるので、古いリソースを破棄。
                m_backFaceDepthTexture.Release();
                m_frontFaceDepthTexture.Release();
                m_finalTexture.Release();
            }

            // 背面の深度値を書き込むテクスチャを作成。
            m_backFaceDepthTexture = new RenderTexture(
                m_depthMapWidth,
                m_depthMapHeight,
                /*depth=*/0,
                RenderTextureFormat.RHalf
            );
            // アンチはいらないので、サンプリング数を1にする。
            m_backFaceDepthTexture.antiAliasing = 1;
            // 表面の深度値を書きこむテクスチャを作成。
            // m_backFaceDepthTextureと同じでいいのでコピーコンストラクタを呼び出す。
            m_frontFaceDepthTexture = new RenderTexture(m_backFaceDepthTexture);

            // 最終描画を行うテクスチャを初期化。
            // モバイルのピクセル処理能力、特にメモリ帯域が厳しいので、
            // レンダリングテクスチャの解像度を1/4にしている。
            m_finalTexture = new RenderTexture(m_depthMapWidth / 4, m_depthMapHeight / 4, 0,
            // todo 最適化のテストのため
            // m_finalTexture = new RenderTexture(m_depthMapWidth, m_depthMapHeight, 0,
                RenderTextureFormat.RGB111110Float);
            // こいつもアンチはいらない。
            m_finalTexture.antiAliasing = 1;

            // 初期化済みの印。
            m_isInitedRenderTexture = true;
        }
    }
}