using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolumeLight
{
    /// <summary>
    /// ボリュームライト描画の共通処理。
    /// </summary>
    public class tkVlit_DrawVolumeLightCommon
    {
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

        public tkVlit_DrawVolumeLightCommon(Camera camera)
        {
            m_camera = camera;
            m_camera.depthTextureMode = DepthTextureMode.Depth;
            m_commandBuffer = new CommandBuffer();
            m_volumeSpotLightList = new List<tkVlit_SpotLight>();
            m_drawBackFaceMaterialList = new List<Material>();
            m_drawFrontFaceMaterialList = new List<Material>();
            m_drawBackMeshFilterList = new List<MeshFilter>();
            m_drawFrontMeshFilterList = new List<MeshFilter>();
            m_drawFinalList = new List<tkVlit_DrawFinal>();
            m_addCopyFullScreen = GameObject.FindObjectOfType<tkLibU_AddCopyFullScreen>();

            m_renderTextures = new tkVlit_RenderTextures();
            m_renderTextures.Init();
        }
        public void AddSpotLight(tkVlit_SpotLight spotLight)
        {
            if (!m_camera)
            {
                return;
            }
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
                if (m_camera.actualRenderingPath == RenderingPath.DeferredShading)
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
        public void RemoveSpotLight(tkVlit_SpotLight spotLight)
        {
            for (int i = 0; i < m_volumeSpotLightList.Count; i++)
            {
                if (m_volumeSpotLightList[i] == spotLight)
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
        public void Release()
        {
            if (m_commandBuffer != null)
            {
                m_commandBuffer.Release();
                m_commandBuffer = null;
            }
        }
        public CommandBuffer Draw(RenderTargetIdentifier cameraRenderTargetID)
        {
            if (m_commandBuffer == null
                || m_camera == null
                || m_renderTextures == null
            )
            {
                return null;
            }

            m_renderTextures.Update();
            // 描画コマンドを構築する。
            tkVlit_BuildDrawVolumeLightCommand.Build(
                m_commandBuffer,
                m_renderTextures,
                m_drawBackFaceMaterialList,
                m_drawFrontFaceMaterialList,
                m_drawBackMeshFilterList,
                m_drawFrontMeshFilterList,
                m_drawFinalList,
                m_volumeSpotLightList,
                m_addCopyFullScreen,
                m_camera,
                cameraRenderTargetID
            );
            return m_commandBuffer;
        }
        public CommandBuffer EndDraw()
        {
            if (m_commandBuffer != null)
            {
                m_commandBuffer.Clear();
            }
            return m_commandBuffer;
        }
    }
}