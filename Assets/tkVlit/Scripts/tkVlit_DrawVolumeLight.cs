using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VolumeLight {
    /// <summary>
    /// ボリュームライト描画処理。
    /// </summary>
    [ExecuteInEditMode]
    public class tkVlit_DrawVolumeLight : MonoBehaviour
    {
        public static tkVlit_DrawVolumeLight instance { get; private set; }
        CommandBuffer m_commandBuffer;                  // コマンドバッファ。
        Camera m_camera;                                // カメラ。
        RenderTexture m_backFaceDepthTexture;           // 背面の深度値が書き込まれているテクスチャ。
        RenderTexture m_frontFaceDepthTexture;          // 表面の深度値が書き込まれているテクスチャ。
        int m_depthMapWidth;                            // 深度マップの幅。
        int m_depthMapHeight;                           // 深度マップの高さ。
        List<tkVlit_SpotLight> m_volumeSpotLightList;   // ボリュームスポットライトのリスト。
        List<Material> m_drawBackFaceMaterialList;      // 背面の深度値描画で使用するマテリアルのリスト。
        List<Material> m_drawFrontFaceMaterialList;     // 表面の深度値描画で使用するマテリアルのリスト。
        List<MeshFilter> m_drawBackMeshFilterList;      // 背面の深度値描画で使用するメッシュフィルターのリスト。
        List<MeshFilter> m_drawFrontMeshFilterList;     // 表面の深度値描画で使用するメッシュフィルターのリスト。
        List<tkVlit_DrawFinal> m_drawFinalList;
#if UNITY_EDITOR
        [MenuItem("Component/tkLibU/tkVlit/tkVlit_DrawVolumeLight")]
        static void OnSelectMenu()
        {
            foreach( var go in Selection.gameObjects)
            {
                go.AddComponent<tkVlit_DrawVolumeLight>();
            }
        }
        [MenuItem("GameObject/tkLibU/tkVlit/tkVlit_SpotLight")]
        static void OnAddSpotLight()
        {
            Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/tkVlit/Prefab/tkVlit_SpotLight.prefab"));
        }
#endif // #if UNITY_EDITOR
        /// <summary>
        /// スポットライトを追加。
        /// </summary>
        /// <param name="spotLight">追加されたスポットライト</param>
        public void AddSpotLight(tkVlit_SpotLight spotLight)
        {
            // 背面の深度値描画に関する初期化処理。
            {
                var trans = spotLight.transform.Find("BackRenderer");
                // Unityのレンダリングパイプラインでは描画しないので、MeshRendererを無効にする。
                var meshRenderer = trans.GetComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                // Unityのレンダリングパイプライン外で描画するためにはマテリアルとメッシュフィルターが必要なので、
                // リストに追加する。
                if (m_camera.actualRenderingPath == RenderingPath.DeferredShading)
                {
                    //
                    meshRenderer.sharedMaterial.EnableKeyword("TK_DEFERRED_PASS");
                }
                else
                {
                    meshRenderer.sharedMaterial.DisableKeyword("TK_DEFERRED_PASS");
                }
                m_drawBackFaceMaterialList.Add(meshRenderer.sharedMaterial);
                m_drawBackMeshFilterList.Add(trans.GetComponent<MeshFilter>());
            }
            // 表面の深度値描画に関する初期化処理。
            {
                var trans = spotLight.transform.Find("FrontRenderer");
                // Unityのレンダリングパイプラインでは描画しないので、MeshRendererを無効にする。
                var meshRenderer = trans.GetComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                // Unityのレンダリングパイプライン外で描画するためにはマテリアルとメッシュフィルターが必要なので、
                // リストに追加する。
                if ( m_camera.actualRenderingPath == RenderingPath.DeferredShading)
                {
                    //
                    meshRenderer.sharedMaterial.EnableKeyword("TK_DEFERRED_PASS");
                }
                else
                {
                    meshRenderer.sharedMaterial.DisableKeyword("TK_DEFERRED_PASS");
                }
                m_drawFrontFaceMaterialList.Add(meshRenderer.sharedMaterial);
                m_drawFrontMeshFilterList.Add(trans.GetComponent<MeshFilter>());
            }
            // 最終描画に関する初期化処理。
            { 
                var trans = spotLight.transform.Find("FinalRenderer");
                m_drawFinalList.Add(trans.GetComponent<tkVlit_DrawFinal>());
            }
            m_volumeSpotLightList.Add(spotLight);
        }
        /// <summary>
        /// スポットライトを削除。
        /// </summary>
        /// <param name="spotLight">削除するスポットライト。</param>
        public void RemoveSpotLight(tkVlit_SpotLight spotLight )
        {
            for( int i = 0; i < m_volumeSpotLightList.Count; i++)
            {
                if( m_volumeSpotLightList[i] == spotLight)
                {
                    m_volumeSpotLightList.RemoveAt(i);
                    m_drawBackFaceMaterialList.RemoveAt(i);
                    m_drawFrontFaceMaterialList.RemoveAt(i);
                    m_drawBackMeshFilterList.RemoveAt(i);
                    m_drawFrontMeshFilterList.RemoveAt(i);
                    m_drawFinalList.RemoveAt(i);
                    break;
                }
            }
        }
        void ReleaseUnmanagedResource()
        {
            if (m_commandBuffer != null)
            {
                m_commandBuffer.Release();
                m_commandBuffer = null;
            }
        }
        private void OnDestroy()
        {
            ReleaseUnmanagedResource();
        }
        private void OnDisable()
        {
            ReleaseUnmanagedResource();
        }
        private void Awake()
        {
            instance = this;
            m_camera = GetComponent<Camera>();
            m_camera.depthTextureMode = DepthTextureMode.Depth;
            m_commandBuffer = new CommandBuffer();
            m_volumeSpotLightList = new List<tkVlit_SpotLight>();
            m_drawBackFaceMaterialList = new List<Material>();
            m_drawFrontFaceMaterialList = new List<Material>();
            m_drawBackMeshFilterList = new List<MeshFilter>();
            m_drawFrontMeshFilterList = new List<MeshFilter>();
            m_drawFinalList = new List<tkVlit_DrawFinal>();
        }
        // Start is called before the first frame update
        void Start()
        {           
            // 深度マップを生成
            m_depthMapWidth = Screen.width;
            m_depthMapHeight = Screen.height;
            m_backFaceDepthTexture = new RenderTexture(
                m_depthMapWidth, 
                m_depthMapHeight, 
                /*depth = */0,
                RenderTextureFormat.RHalf
            );
            m_frontFaceDepthTexture = new RenderTexture(m_backFaceDepthTexture);
        }
        // Update is called once per frame
        void OnPreRender()
        {
            if( m_commandBuffer == null
                || m_camera == null
            ){
                return;
            }
            if (m_depthMapWidth != Screen.width || m_depthMapHeight != Screen.height)
            {
                // 画面解像度が変わったので作り直し。
                m_depthMapWidth = Screen.width;
                m_depthMapHeight = Screen.height;
                m_backFaceDepthTexture = new RenderTexture(m_depthMapWidth, m_depthMapHeight, 0, RenderTextureFormat.RHalf);
                m_frontFaceDepthTexture = new RenderTexture(m_backFaceDepthTexture);
            }

            // ボリュームライトの最終描画でメインシーンの描画結果のテクスチャを利用したいのだが、
            // レンダリングターゲットとして指定されているテクスチャを読み込みで利用することはできないので、
            // 一時的なレンダリングターゲットを取得してそこにコピーする。
            int cameraTargetTextureID = Shader.PropertyToID("cameraTargetTexture");
            m_commandBuffer.GetTemporaryRT(
                cameraTargetTextureID, 
                /*width=*/-1,  // -1ならCamera pixel widthと同じになる。
                /*height=*/-1, // -1ならCamera pixel heightと同じになる。
                /*depth=*/0,   
                FilterMode.Bilinear
            );
            m_commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, cameraTargetTextureID);

            // 全てにボリュームライトを描画していく。
            for ( int litNo = 0; litNo < m_drawBackFaceMaterialList.Count; litNo++)
            {
                Matrix4x4 mWorld = Matrix4x4.TRS(
                    m_drawBackMeshFilterList[litNo].transform.position,
                    m_drawBackMeshFilterList[litNo].transform.rotation,
                    m_drawBackMeshFilterList[litNo].transform.lossyScale
                );
                // 背面の深度値を描画。
                m_commandBuffer.SetRenderTarget(m_backFaceDepthTexture);
                // todo プラットフォームによってはクリアする値を変更する必要があるかも。
                m_commandBuffer.ClearRenderTarget(true, true, Color.white);
                m_commandBuffer.DrawMesh(
                    m_drawBackMeshFilterList[litNo].sharedMesh,
                    mWorld,
                    m_drawBackFaceMaterialList[litNo]
                );

                // 表面の深度値を描画。
                m_commandBuffer.SetRenderTarget(m_frontFaceDepthTexture);
                // todo プラットフォームによってはクリアする値を変更する必要があるかも。
                m_commandBuffer.ClearRenderTarget(true, true, Color.white);
                m_commandBuffer.DrawMesh(
                    m_drawFrontMeshFilterList[litNo].sharedMesh,
                    mWorld,
                    m_drawFrontFaceMaterialList[litNo]
                );
                
                m_commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

                m_drawFinalList[litNo].Draw(
                    m_camera,
                    m_frontFaceDepthTexture,
                    m_backFaceDepthTexture,
                    m_commandBuffer,
                    m_volumeSpotLightList[litNo].volumeSpotLightData
                );

            }
            m_camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, m_commandBuffer);
        }
        private void OnPostRender()
        {
            if (m_commandBuffer == null
                || m_camera == null
            )
            {
                return;
            }
            m_camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, m_commandBuffer);
            m_commandBuffer.Clear();
        }
    }
}