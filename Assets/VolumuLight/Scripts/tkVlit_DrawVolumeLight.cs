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
        CommandBuffer m_commandBuffer;                  // コマンドバッファ。
        Camera m_camera;                                // カメラ。
        tkVlit_SpotLight[] m_volumeSpotLights;           // ボリュームスポットライト。
        RenderTexture m_backFaceDepthTexture;           // 背面の深度値が書き込まれているテクスチャ。
        RenderTexture m_frontFaceDepthTexture;          // 表面の深度値が書き込まれているテクスチャ。
        int m_depthMapWidth;                            // 深度マップの幅。
        int m_depthMapHeight;                           // 深度マップの高さ。
        List<Material> m_drawBackFaceMaterialList;      // 背面の深度値描画で使用するマテリアルのリスト。
        List<Material> m_drawFrontFaceMaterialList;     // 表面の深度値描画で使用するマテリアルのリスト。
        List<MeshFilter> m_drawBackMeshFilterList;      // 背面の深度値描画で使用するメッシュフィルターのリスト。
        List<MeshFilter> m_drawFrontMeshFilterList;     // 表面の深度値描画で使用するメッシュフィルターのリスト。
        List<tkVlit_DrawFinal> m_drawFinals;

        [MenuItem("Component/tkVlit/tkVlit_DrawVolumeLight")]
        static void OnSelectMenu()
        {
            foreach( var go in Selection.gameObjects)
            {
                go.AddComponent<tkVlit_DrawVolumeLight>();
            }
        }
        [MenuItem("GameObject/tkVlit/tkVlit_SpotLight")]
        static void OnAddSpotLight()
        {
            
        }
        // Start is called before the first frame update
        void Start()
        {
            m_camera = GetComponent<Camera>();
            m_camera.depthTextureMode = DepthTextureMode.Depth;
            m_commandBuffer = new CommandBuffer();
            m_volumeSpotLights = Object.FindObjectsOfType<tkVlit_SpotLight>();
            m_drawBackFaceMaterialList = new List<Material>();
            m_drawFrontFaceMaterialList = new List<Material>();
            m_drawBackMeshFilterList = new List<MeshFilter>();
            m_drawFrontMeshFilterList = new List<MeshFilter>();
            m_drawFinals = new List<tkVlit_DrawFinal>();
            foreach ( var volumeSpotLight in m_volumeSpotLights)
            {
                // 背面の深度値を描画するためのゲームオブジェクトを取得。
                var trans = volumeSpotLight.transform.Find("BackRenderer");
                // Unityのレンダリングパイプラインでは描画しないので、MeshRendererを無効にする。
                var meshRenderer = trans.GetComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                // マテリアルとメッシュフィルターを集める。
                m_drawBackFaceMaterialList.Add(meshRenderer.sharedMaterial);
                m_drawBackMeshFilterList.Add(trans.GetComponent<MeshFilter>());

                // 表面の深度値を描画するためのゲームオブジェクトを取得。
                trans = volumeSpotLight.transform.Find("FrontRenderer");
                // Unityのレンダリングパイプラインでは描画しないので、MeshRendererを無効にする。
                meshRenderer = trans.GetComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                // マテリアルとメッシュフィルターを集める。
                m_drawFrontFaceMaterialList.Add(meshRenderer.sharedMaterial);
                m_drawFrontMeshFilterList.Add(trans.GetComponent<MeshFilter>());

                // 最終描画のゲームオブジェクトを取得。
                trans = volumeSpotLight.transform.Find("FinalRenderer");
                m_drawFinals.Add(trans.GetComponent<tkVlit_DrawFinal>());
            }
            // 深度マップを生成
            m_depthMapWidth = Screen.width;
            m_depthMapHeight = Screen.height;
            m_backFaceDepthTexture = new RenderTexture(m_depthMapWidth, m_depthMapHeight, 0, RenderTextureFormat.RHalf);
            m_frontFaceDepthTexture = new RenderTexture(m_backFaceDepthTexture);
        }
        // Update is called once per frame
        void OnPreRender()
        {
            if (m_depthMapWidth != Screen.width || m_depthMapHeight != Screen.height)
            {
                // 画面解像度が変わったので作り直し。
                m_depthMapWidth = Screen.width;
                m_depthMapHeight = Screen.height;
                m_backFaceDepthTexture = new RenderTexture(m_depthMapWidth, m_depthMapHeight, 0, RenderTextureFormat.RHalf);
                m_frontFaceDepthTexture = new RenderTexture(m_backFaceDepthTexture);
            }

            // カメラターゲットのテクスチャを一時テクスチャにコピーする。
            int cameraTargetTextureID = Shader.PropertyToID("cameraTargetTexture");
            // 一時的なレンダテクスチャを作成
            m_commandBuffer.GetTemporaryRT(cameraTargetTextureID, -1, -1, 0, FilterMode.Bilinear);
            // CameraTargetを一時的なレンダテクスチャコピー。
            m_commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, cameraTargetTextureID);

            
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

                m_drawFinals[litNo].Draw(
                    m_camera,
                    m_frontFaceDepthTexture,
                    m_backFaceDepthTexture,
                    m_commandBuffer,
                    m_volumeSpotLights[litNo].volumeSpotLightData
                );

            }
            m_camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, m_commandBuffer);
        }
        private void OnPostRender()
        {
            m_camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, m_commandBuffer);
            m_commandBuffer.Clear();
        }
    }
}