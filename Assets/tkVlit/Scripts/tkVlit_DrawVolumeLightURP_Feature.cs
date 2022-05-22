using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VolumeLight
{
    public class tkVlit_DrawVolumeLightURP_Feature : ScriptableRendererFeature
    {
        
        tkVlit_DrawVolumeLightURP m_drawVolumeLightURP;
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_drawVolumeLightURP.cameraRenderTargetID = renderer.cameraColorTarget;
            renderer.EnqueuePass(m_drawVolumeLightURP);
        }

        public override void Create()
        {
            m_drawVolumeLightURP = new tkVlit_DrawVolumeLightURP();
        }
    }
}