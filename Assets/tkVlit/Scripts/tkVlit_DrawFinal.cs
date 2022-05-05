using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace VolumeLight
{
    /// <summary>
    /// ボリュームスポットライトのデータ構造体。
    /// </summary>
    
    public struct VolumeSpotLightData
    {
        public Vector3 position;           // 座標
        public int isUse;                  // 使用中フラグ。
        public Vector3 positionInView;     // カメラ空間での座標。
        public int no;                     // ライトの番号。
        public Vector3 direction;          // 射出方向。
        public Vector3 range;              // 影響範囲。
        public Vector3 color;              // ライトのカラー。
        public Vector3 color2;             // 二つ目のカラー。
        public Vector3 color3;             // 三つ目のカラー。
        public Vector3 directionInView;    // カメラ空間での射出方向。
        public Vector3 rangePow;           // 距離による光の影響率に累乗するパラメーター。1.0で線形の変化をする。
                                           // xが一つ目のカラー、yが二つ目のカラー、zが三つ目のカラー。
        public Vector3 halfAngleRad;       // 射出角度(単位：ラジアン。xが一つ目のカラー、yが二つ目のカラー、zが三つ目のカラー)。
        public Vector3 anglePow;           // スポットライトとの角度による光の影響率に累乗するパラメータ。1.0で線形に変化する。
                                           // xが一つ目のカラー、yが二つ目のカラー、zが三つ目のカラー。
    };
    /// <summary>
    /// ボリュームライト描画の最後の描画処理。
    /// </summary>
    [ExecuteInEditMode]
    public class tkVlit_DrawFinal : MonoBehaviour
    {
        /// <summary>
        /// シェーダープロパティIDをまとめた構造体
        /// </summary>
        struct ShaderPropertyToID
        {
            public int volumeFrontTexID;            // 表面の深度値を書き込んだテクスチャ。
            public int volumeBackTexID;             // 背面の深度値を書き込んだテクスチャ。
            public int viewProjectionMatrixInvID;   // ビュープロジェクション行列の逆行列。
            public int randomSeedID;                // ランダムシード。
            public int volumeSpotLightArrayID;      // ボリュームスポットライトの配列。
        };

        MeshFilter m_meshFilter;                    // メッシュフィルター。
        Material m_material;                        // マテリアル。
        ShaderPropertyToID m_shaderPropToId = new ShaderPropertyToID();
        VolumeSpotLightData[] m_volumeSpotLightDataArray = new VolumeSpotLightData[1];
        GraphicsBuffer m_volumeSpotLightDataGraphicsBuffer;

        private void OnDestroy()
        {
            DestroyImmediate(m_material);
        }
        void Start()
        {
            // スポットライトのデータを送るためのストラクチャードバッファを作成。
            m_volumeSpotLightDataGraphicsBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                1,
                Marshal.SizeOf(typeof(VolumeSpotLightData))
            );
            // Unityのデフォルトの描画パスでは描画しないのでメッシュレンダラーを無効にする。
            var meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            m_meshFilter = GetComponent<MeshFilter>();
            m_material = new Material( meshRenderer.sharedMaterial);
            
            // 各種シェーダーパラメータのIDを取得する。
            m_shaderPropToId.volumeFrontTexID = Shader.PropertyToID("volumeFrontTexture");
            m_shaderPropToId.volumeBackTexID = Shader.PropertyToID("volumeBackTexture");
            m_shaderPropToId.viewProjectionMatrixInvID = Shader.PropertyToID("viewProjMatrixInv");
            m_shaderPropToId.randomSeedID = Shader.PropertyToID("ramdomSeed");
            m_shaderPropToId.volumeSpotLightArrayID = Shader.PropertyToID("volumeSpotLightArray");

            
        }
        
        public void Draw(Camera camera, RenderTexture volumeMapFront, RenderTexture VolumeMapBack, CommandBuffer commandBuffer, VolumeSpotLightData data)
        {
            m_volumeSpotLightDataArray[0] = data;
            m_volumeSpotLightDataGraphicsBuffer.SetData(m_volumeSpotLightDataArray);
            // シェーダーで使用されるプロジェクション行列を取得する。
            // -> OpenGLとDirectXでクリップ座標系が異なるため、実際にシェーダー側に送られるプロジェクション行列は加工が加えられている。
            //    なので、実際にシェーダー側で使用するプロジェクション行列を取得するには、GL.GetGPUProjectionMatrix()を使う。
            var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
            // シェーダー側で正規化スクリーン座標系からワールド座標系に復元する処理があるので、ここでビュープロジェクション行列の逆行列を計算する。
            Matrix4x4 mViewProjMatInv = projMatrix * camera.worldToCameraMatrix;
            mViewProjMatInv = Matrix4x4.Inverse(mViewProjMatInv);

            Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, transform.lossyScale);

            // テクスチャや行列などのシェーダーリソースをマテリアルに設定する。
            m_material.SetTexture(m_shaderPropToId.volumeFrontTexID, volumeMapFront);
            m_material.SetTexture(m_shaderPropToId.volumeBackTexID, VolumeMapBack);
            m_material.SetMatrix(m_shaderPropToId.viewProjectionMatrixInvID, mViewProjMatInv);
            m_material.SetFloat(m_shaderPropToId.randomSeedID, Random.Range(0.0f, 1.0f));
            m_material.SetBuffer(m_shaderPropToId.volumeSpotLightArrayID, m_volumeSpotLightDataGraphicsBuffer);

            // ドロー。
            commandBuffer.DrawMesh(
                 m_meshFilter.sharedMesh,
                 m,
                 m_material
             );
        }
    }
}