

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
        tkVlit_RenderTextures m_renderTextures;         // レンダリングテクスチャたち。
        tkLibU_AddCopyFullScreen m_addCopyFullScreen;   // 全画面にフルスクリーンコピー。
        List<tkVlit_SpotLight> m_volumeSpotLightList;   // ボリュームスポットライトのリスト。
        List<Material> m_drawBackFaceMaterialList;      // 背面の深度値描画で使用するマテリアルのリスト。
        List<Material> m_drawFrontFaceMaterialList;     // 表面の深度値描画で使用するマテリアルのリスト。
        List<MeshFilter> m_drawBackMeshFilterList;      // 背面の深度値描画で使用するメッシュフィルターのリスト。
        List<MeshFilter> m_drawFrontMeshFilterList;     // 表面の深度値描画で使用するメッシュフィルターのリスト。
        List<tkVlit_DrawFinal> m_drawFinalList;
#if UNITY_EDITOR
        /// <summary>
        /// TKLibUの初期化処理。
        /// </summary>
        static void InitTKLibU()
        {
            var commonObject = GameObject.Find("tkLibU_Common");
            if(commonObject == null)
            {
                // tkLibUの共通オブジェクトが設置されていないのでシーンに追加する。
                var obj = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/tkLibU_Common/Prefab/tkLibU_Common.prefab"));
                obj.name = obj.name.Replace("(Clone)", "");
            }
        }
        [MenuItem("Component/tkLibU/tkVlit/tkVlit_DrawVolumeLight")]
        static void OnSelectMenu()
        {
            InitTKLibU();
            foreach ( var go in Selection.gameObjects)
            {
                go.AddComponent<tkVlit_DrawVolumeLight>();
            }
        }
        [MenuItem("GameObject/tkLibU/tkVlit/tkVlit_SpotLight")]
        static void OnAddSpotLight()
        {
            InitTKLibU();
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
            m_addCopyFullScreen = GameObject.FindObjectOfType< tkLibU_AddCopyFullScreen>();
        }
        
       
        // Start is called before the first frame update
        void Start()
        {
            m_renderTextures = new tkVlit_RenderTextures();
            m_renderTextures.Init();
        }
        // Update is called once per frame
        void OnPreRender()
        {
            if( m_commandBuffer == null
                || m_camera == null
            ){
                return;
            }

            m_renderTextures.Update();

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

            m_commandBuffer.SetRenderTarget(m_renderTextures.finalTexture);
            m_commandBuffer.ClearRenderTarget(
                /*clearDepth=*/true,
                /*clearColor=*/true,
                Color.black
            );

            // 全てにボリュームライトを描画していく。
            for ( int litNo = 0; litNo < m_drawBackFaceMaterialList.Count; litNo++)
            {
                Matrix4x4 mWorld = Matrix4x4.TRS(
                    m_drawBackMeshFilterList[litNo].transform.position,
                    m_drawBackMeshFilterList[litNo].transform.rotation,
                    m_drawBackMeshFilterList[litNo].transform.lossyScale
                );
                // 背面の深度値を描画。
                m_commandBuffer.SetRenderTarget(m_renderTextures.backFaceDepthTexture);
                // todo プラットフォームによってはクリアする値を変更する必要があるかも。
                m_commandBuffer.ClearRenderTarget(true, true, Color.white);
                m_commandBuffer.DrawMesh(
                    m_drawBackMeshFilterList[litNo].sharedMesh,
                    mWorld,
                    m_drawBackFaceMaterialList[litNo]
                );

                // 表面の深度値を描画。
                m_commandBuffer.SetRenderTarget(m_renderTextures.frontFaceDepthTexture);
                // todo プラットフォームによってはクリアする値を変更する必要があるかも。
                m_commandBuffer.ClearRenderTarget(true, true, Color.white);
                m_commandBuffer.DrawMesh(
                    m_drawFrontMeshFilterList[litNo].sharedMesh,
                    mWorld,
                    m_drawFrontFaceMaterialList[litNo]
                );
// todo #if DRAW_FINAL_DOWN_SCALE
                m_commandBuffer.SetRenderTarget(m_renderTextures.finalTexture);
// todo #else // #if DRAW_FINAL_DOWN_SCALE
// todo         m_commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
// todo #endif // #if DRAW_FINAL_DOWN_SCALE
                m_drawFinalList[litNo].Draw(
                    m_camera,
                    m_renderTextures.frontFaceDepthTexture,
                    m_renderTextures.backFaceDepthTexture,
                    m_commandBuffer,
                    m_volumeSpotLightList[litNo].volumeSpotLightData
                );
            }
// todo #if DRAW_FINAL_DOWN_SCALE
            m_commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            if(m_drawFinalList != null
                && m_drawFinalList.Count > 0
                && m_addCopyFullScreen != null)
            {
                m_addCopyFullScreen.Draw(
                    m_commandBuffer,
                    m_renderTextures.finalTexture,
                    BuiltinRenderTextureType.CameraTarget
                );
            }
            //m_commandBuffer.Blit(m_finalTexture, BuiltinRenderTextureType.CameraTarget, m_copyAddMatrial);
// todo #endif // #if DRAW_FINAL_DOWN_SCALE
            m_camera.forceIntoRenderTexture = true;
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