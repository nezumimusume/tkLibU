using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Rendering;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector3 = UnityEngine.Vector3;

namespace tkLibU
{

    /// <summary>
    /// 平面リフレクションのキャスターコンポーネント
    /// </summary>
    public class tkPlRef_PlaneReflectionCaster : MonoBehaviour
    {
        private List<Renderer> m_meshRendererList = new List<Renderer>();

        private List<Material> m_materialList = new List<Material>();

        private List<Mesh> m_meshList = new List<Mesh>();
        // Start is called before the first frame update
        void Start()
        {
            var casterMaterial = tkPlRef_PlaneReflectionPass.instance.casterMaterial;
            var rendererArray = transform.GetComponentsInChildren<Renderer>();
            foreach ( var renderer in rendererArray)
            {
                bool isSkinnedMeshRenderer = renderer is SkinnedMeshRenderer;
                bool isMeshRenderer = renderer is MeshRenderer;
                if (isSkinnedMeshRenderer == false
                    && isMeshRenderer == false)
                {
                    continue;
                }
                // メッシュを収集する。
                if (isSkinnedMeshRenderer)
                {
                    var skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                    m_meshList.Add(skinnedMeshRenderer.sharedMesh);
                    
                }else
                {
                    m_meshList.Add((renderer.GetComponent<MeshFilter>().sharedMesh));
                }

                var material = new Material(casterMaterial);
                material.mainTexture = renderer.sharedMaterial.mainTexture;
                m_materialList.Add(material);
                m_meshRendererList.Add(renderer);
            }
        }

        // Update is called once per frame
        void Update()
        {
            var pass = tkPlRef_PlaneReflectionPass.instance;
            pass?.RegisterPlaneReflectionCaster(this);
        }
        
        public void Draw(CommandBuffer commandBuffer, tkPlRef_PlaneReflectionReceiver reciever)
        {
            // 平面空間に変換する行列を作成する。
            // まずは平面空間の規定軸を求める。
            Vector3 axisX = new Vector3();
            Vector3 axisY = new Vector3();
            Vector3 axisZ = new Vector3();
            // 平面の上方向がZ軸になる。
            axisZ = reciever.transform.rotation * Vector3.up;
            axisX = reciever.transform.rotation * Vector3.right;
            axisY = reciever.transform.rotation * Vector3.forward;
            
            Matrix4x4 planeToWorldSpaceMatrix = new Matrix4x4();
            planeToWorldSpaceMatrix.SetColumn(0, axisX.normalized);
            planeToWorldSpaceMatrix.SetColumn(1, axisY.normalized);
            planeToWorldSpaceMatrix.SetColumn(2, axisZ.normalized);
            UnityEngine.Vector4 pos = reciever.transform.position;
            pos.w = 1.0f;
            planeToWorldSpaceMatrix.SetColumn(3, pos);
            var worldToPlaneSpaceMatrix = Matrix4x4.Inverse(planeToWorldSpaceMatrix);

            for( int i = 0; i < m_materialList.Count; i++)
            {
                var mesh = m_meshList[i];
                var transform = m_meshRendererList[i].transform;
                var m = Matrix4x4.TRS(
                    transform.position, 
                    transform.rotation, 
                    transform.lossyScale );

                var mScaleY = Matrix4x4.Scale(new Vector3(1.0f, 1.0f, -1.0f));
                m = planeToWorldSpaceMatrix * mScaleY * worldToPlaneSpaceMatrix * m;
                
                commandBuffer.DrawMesh(
                    mesh,
                    m,
                    m_materialList[i] );
            }    
        }
    }
}