

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace tkLibU
{
    /// <summary>
    /// ビルトインレンダリングパイプラインでのボリュームライト描画処理。
    /// </summary>
    [ExecuteInEditMode]
    public class tkVlit_DrawVolumeLightBRP : MonoBehaviour
    {
        public static tkVlit_DrawVolumeLightBRP instance { get; private set; }
        tkVlit_DrawVolumeLightCommon m_drawVolumeLightCommon;    // ボリュームライト描画の共通処理。
        Camera m_camera;                                // カメラ。

        public void AddSpotLight(tkVlit_SpotLight spotLight)
        {
            m_drawVolumeLightCommon?.AddSpotLight(spotLight);
        }
        public void RemoveSpotLight(tkVlit_SpotLight spotLight )
        {
            m_drawVolumeLightCommon?.RemoveSpotLight(spotLight);
        }
        private void OnDestroy()
        {
            m_drawVolumeLightCommon?.Release();
        }
        private void OnDisable()
        {
            m_drawVolumeLightCommon?.Release();
        }
        private void Awake()
        {
            instance = this;
            m_camera = GetComponent<Camera>();
            m_drawVolumeLightCommon = new tkVlit_DrawVolumeLightCommon(m_camera);
        }
        
        // Update is called once per frame
        void OnPreRender()
        {
            var commandBuffer = m_drawVolumeLightCommon?.Draw(BuiltinRenderTextureType.CameraTarget);
            m_camera.forceIntoRenderTexture = true;
            if (commandBuffer != null)
            {
                m_camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBuffer);
            }
        }
        private void OnPostRender()
        {
            if (m_drawVolumeLightCommon == null
                || m_camera == null
            )
            {
                return;
            }
            var commandBuffer = m_drawVolumeLightCommon.EndDraw();
            m_camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBuffer);
        }
    }
}