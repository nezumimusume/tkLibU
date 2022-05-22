

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolumeLight {
    /// <summary>
    /// URPでのボリュームライト描画処理。
    /// </summary>    
    public class tkVlit_DrawVolumeLightURP : ScriptableRenderPass
    {
        tkVlit_DrawVolumeLightCommon m_drawVolumeLightCommon;   // ボリュームライトの描画処理の共通処理。
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            throw new System.NotImplementedException();
        }
    }
}