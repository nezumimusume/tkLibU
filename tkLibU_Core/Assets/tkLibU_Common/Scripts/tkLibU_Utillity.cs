using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tkLibU
{
    public class tkLibU_Utillity
    {
        public static void QueryMaterialInChildren(
            Transform rootTransform,
            Func<Material, Material> query)
        {
            var rendererArray = rootTransform.GetComponentsInChildren<Renderer>();
            foreach (var renderer in rendererArray)
            {
                bool isSkinnedMeshRenderer = renderer is SkinnedMeshRenderer;
                bool isMeshRenderer = renderer is MeshRenderer;
                if (isSkinnedMeshRenderer == false
                    && isMeshRenderer == false)
                {
                    continue;
                }

                renderer.sharedMaterial = query(renderer.sharedMaterial);
            }
        }
    }
}
