using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tkLibU
{
    public class tkPlRef_PlaneReflectionReceiver : MonoBehaviour
    {
        [SerializeField]
        private RenderTexture m_reflectionTexture;
        public RenderTexture reflectionTexture
        {
            get => m_reflectionTexture;
        }
        [SerializeField] private int m_reflectionTextureWidth = 512;
        [SerializeField] private int m_reflectionTextureHeight = 512;

        // Start is called before the first frame update
        void Start()
        {
            int w = Mathf.Max(m_reflectionTextureWidth, 2);
            int h = Mathf.Min(m_reflectionTextureHeight, 2);
            m_reflectionTexture = new RenderTexture(
                m_reflectionTextureWidth,
                m_reflectionTextureHeight,
                32,
                RenderTextureFormat.RGB111110Float
            );
            var srcMaterial = tkPlRef_PlaneReflectionPass.instance.receiverMaterial;
            // レシーバーオブジェクトのマテリアルを変更していく。
            tkLibU_Utillity.QueryMaterialInChildren(
                transform,
                (mat) =>
                {
                    // マテリアルを差し替える。
                    var newMaterial = new Material(srcMaterial);
                    newMaterial.mainTexture = mat.mainTexture;
                    newMaterial.SetTexture("_planeReflectionTex", m_reflectionTexture);
                    return newMaterial;
                });
        }

        // Update is called once per frame
        void Update()
        {
            var pass = tkPlRef_PlaneReflectionPass.instance;
            pass?.RegisterPlaneReflectionReceiver(this);
        }
    }
}