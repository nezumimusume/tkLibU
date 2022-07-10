using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using tkLibU;

#if UNITY_EDITOR

namespace tkLibU
{
    public class tkVlit_Editor
    {
        /// <summary>
        /// TKLibUの初期化処理。
        /// </summary>
        static void InitTKLibU()
        {
            var commonObject = GameObject.Find("tkLibU_Common");
            if (commonObject == null)
            {
                // tkLibUの共通オブジェクトが設置されていないのでシーンに追加する。
                var obj = Object.Instantiate(
                    AssetDatabase.LoadAssetAtPath<GameObject>("Assets/tkLibU_Common/Prefab/tkLibU_Common.prefab"));
                obj.name = obj.name.Replace("(Clone)", "");
            }
        }

        [MenuItem("Component/tkLibU/tkVlit/tkVlit_DrawVolumeLight")]
        static void OnSelectMenu()
        {
            InitTKLibU();
            foreach (var go in Selection.gameObjects)
            {
                go.AddComponent<tkVlit_DrawVolumeLightBRP>();
            }
        }

        [MenuItem("GameObject/tkLibU/tkVlit/tkVlit_SpotLight")]
        static void OnAddSpotLight()
        {
            InitTKLibU();
            Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/tkVlit/Prefab/tkVlit_SpotLight.prefab"));
        }
    }
}
#endif // #if UNITY_EDITOR