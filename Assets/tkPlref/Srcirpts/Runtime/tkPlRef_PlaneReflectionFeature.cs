using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace tkLibU
{
    /// <summary>
    /// The feature that create new rendering pipeline. 
    /// </summary>
    public class tkPlRef_PlaneReflectionFeature : ScriptableRendererFeature
    {
        private tkPlRef_PlaneReflectionPass m_planeReflectionPass;
        [SerializeField]
        private Material m_casterMaterial;
        [SerializeField]
        private Material m_receiverMaterial;
        public override void Create()
        {
            m_planeReflectionPass = new tkPlRef_PlaneReflectionPass();
            m_planeReflectionPass.casterMaterial = m_casterMaterial;
            m_planeReflectionPass.receiverMaterial = m_receiverMaterial;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_planeReflectionPass);
        }
    }
}