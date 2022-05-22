

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;



namespace VolumeLight {
    /// <summary>
    /// ビルトインレンダリングパイプラインでのボリュームライト描画処理。
    /// </summary>
    [ExecuteInEditMode]
    public class tkVlit_DrawVolumeLightBRP : MonoBehaviour
    {
        public static tkVlit_DrawVolumeLightBRP instance { get; private set; }
        tkVlit_DrawVolumeLightCommon m_drawVolumeLightCommon;    // ボリュームライト描画の共通処理。
        Camera m_camera;                                // カメラ。
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
                go.AddComponent<tkVlit_DrawVolumeLightBRP>();
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
            m_drawVolumeLightCommon?.AddSpotLight(spotLight);
        }
        /// <summary>
        /// スポットライトを削除。
        /// </summary>
        /// <param name="spotLight">削除するスポットライト。</param>
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
            var commandBuffer = m_drawVolumeLightCommon?.Draw();
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