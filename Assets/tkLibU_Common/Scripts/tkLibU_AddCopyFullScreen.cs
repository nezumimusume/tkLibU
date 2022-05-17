using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class tkLibU_AddCopyFullScreen : MonoBehaviour
{
    [SerializeField]
    Material m_addCopyMaterial;
    MeshFilter m_planeMeshFilter;
    int m_srcTextureShaderID;
    // Start is called before the first frame update
    void Start()
    {
        m_planeMeshFilter = GetComponent<MeshFilter>();
        m_srcTextureShaderID = Shader.PropertyToID("srcTexture");
    }
    /// <summary>
    /// 描画
    /// </summary>
    /// <param name="commandBuffer">コマンドバッファ</param>
    /// <param name="srcTexture">コピー元のテクスチャ</param>
    /// <param name="renderTextureType">コピー先のレンダーテクスチャ</param>
    public void Draw(
        CommandBuffer commandBuffer,
        Texture srcTexture,
        BuiltinRenderTextureType renderTextureType
    )
    {
        if(m_planeMeshFilter == null
            || m_addCopyMaterial == null
        )
        {
            return;
        }
        m_addCopyMaterial.SetTexture(m_srcTextureShaderID, srcTexture);
        commandBuffer.SetRenderTarget(renderTextureType);
        commandBuffer.DrawMesh(
            m_planeMeshFilter.sharedMesh,
            Matrix4x4.identity,
            m_addCopyMaterial
        );
    }
}
