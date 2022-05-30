using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace tkLibU
{
    /// <summary>
    /// It is rendering pass that add to URP.
    /// </summary>
    public class tkPlRef_PlaneReflectionPass : ScriptableRenderPass
    {
        private static tkPlRef_PlaneReflectionPass m_instance;
        private List<tkPlRef_PlaneReflectionReceiver> m_planeReflectionReceiverList =
            new List<tkPlRef_PlaneReflectionReceiver>();

        private List<tkPlRef_PlaneReflectionCaster> m_planeReflectionCasterList =
            new List<tkPlRef_PlaneReflectionCaster>();

        
        private CommandBuffer m_commandBuffer = new CommandBuffer();
        public Material casterMaterial { get; set; }
        public Material receiverMaterial { get; set; }
        public static tkPlRef_PlaneReflectionPass instance
        {
            get => m_instance;
        }
        public tkPlRef_PlaneReflectionPass()
        {
            m_instance = this;
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }
        
        public void RegisterPlaneReflectionReceiver(tkPlRef_PlaneReflectionReceiver receiver)
        {
            m_planeReflectionReceiverList.Add(receiver);
        }

        public void RegisterPlaneReflectionCaster(tkPlRef_PlaneReflectionCaster caster)
        {
            m_planeReflectionCasterList.Add(caster);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            foreach (var recv in m_planeReflectionReceiverList)
            {
                // レシーバーにキャスターを描画していく。
                m_commandBuffer.SetRenderTarget(recv.reflectionTexture);
                m_commandBuffer.ClearRenderTarget(true, true,Color.black,1.0f);
                foreach (var caster in m_planeReflectionCasterList)
                {
                    caster.Draw(m_commandBuffer, recv);
                }
            }
            context.ExecuteCommandBuffer(m_commandBuffer);
            m_planeReflectionCasterList.Clear();
            m_planeReflectionReceiverList.Clear();
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            m_commandBuffer.Clear();
        }
    }
}