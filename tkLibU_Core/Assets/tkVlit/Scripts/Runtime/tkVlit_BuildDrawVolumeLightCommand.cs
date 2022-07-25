#define CALC_BOUND
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace tkLibU
{
    /// <summary>
    /// ボリュームライト描画ためのコマンドを構築する処理。
    /// </summary>
    public class tkVlit_BuildDrawVolumeLightCommand
    {
        static Vector3[] vertices;
        public static void Build(
            CommandBuffer commandBuffer,
            tkVlit_RenderTextures renderTextures,
            List<Material> drawBackFaceMaterialList,
            List<Material> drawFrontFaceMaterialList,
            List<MeshFilter> drawBackMeshFilterList,
            List<MeshFilter> drawFrontMeshFilterList,
            List<tkVlit_DrawFinal> drawFinalList,
            List<tkVlit_SpotLight> volumeSpotLightList,
            tkLibU_AddCopyFullScreen addCopyFullScreen,
            Camera camera,
            RenderTargetIdentifier cameraRenderTargetID
        )
        {
            if(drawBackMeshFilterList.Count == 0) {
                return;
            }
            // ボリュームライトの最終描画でメインシーンの描画結果のテクスチャを利用したいのだが、
            // レンダリングターゲットとして指定されているテクスチャを読み込みで利用することはできないので、
            // 一時的なレンダリングターゲットを取得してそこにコピーする。
            int cameraTargetTextureID = Shader.PropertyToID("cameraTargetTexture");
            commandBuffer.GetTemporaryRT(
                cameraTargetTextureID,
                camera.pixelWidth,
                camera.pixelHeight,
                /*depth=*/0,
                FilterMode.Bilinear
            );
            commandBuffer.Blit(cameraRenderTargetID, cameraTargetTextureID);
            commandBuffer.SetRenderTarget(renderTextures.finalTexture);
            commandBuffer.ClearRenderTarget(
                /*clearDepth=*/true,
                /*clearColor=*/true,
                Color.black
            );

            // 全てにボリュームライトを描画していく。
            for (int litNo = 0; litNo < drawBackFaceMaterialList.Count; litNo++)
            {

                Matrix4x4 mWorld = Matrix4x4.TRS(
                    drawBackMeshFilterList[litNo].transform.position,
                    drawBackMeshFilterList[litNo].transform.rotation,
                    drawBackMeshFilterList[litNo].transform.lossyScale
                );
              
                // 背面の深度値を描画。
                commandBuffer.SetRenderTarget(renderTextures.backFaceDepthTexture);
                // todo プラットフォームによってはクリアする値を変更する必要があるかも。
                commandBuffer.ClearRenderTarget(true, true, Color.white);
                commandBuffer.DrawMesh(
                    drawBackMeshFilterList[litNo].sharedMesh,
                    mWorld,
                    drawBackFaceMaterialList[litNo]
                );

                // 表面の深度値を描画。
                commandBuffer.SetRenderTarget(renderTextures.frontFaceDepthTexture);
                commandBuffer.ClearRenderTarget(true, true, Color.white);
                commandBuffer.DrawMesh(
                    drawFrontMeshFilterList[litNo].sharedMesh,
                    mWorld,
                    drawFrontFaceMaterialList[litNo]
                );
#if CALC_BOUND
                if(vertices == null)
                {
                    vertices = drawBackMeshFilterList[0].sharedMesh.vertices;
                }
                
                // 8頂点をワールド空間に変換する。
                var aabbMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                var aabbMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);

                Matrix4x4 mvp = projMatrix * camera.worldToCameraMatrix * mWorld;

                for( int i = 0; i < vertices.Length; i++) { 
                
                    Vector3 v = mvp.MultiplyPoint(vertices[i]); 
                    aabbMin = Vector3.Max(Vector3.one * -1.0f, Vector3.Min(v, aabbMin));
                    aabbMax = Vector3.Min(Vector3.one, Vector3.Max(v, aabbMax));
                }
#else
                var bounds = drawBackMeshFilterList[0].sharedMesh.bounds;
                // バウンディングボックスを構築する8頂点を計算する。
                Vector3[] boundsVertexPositions = new Vector3[8];
                // 最小値
                boundsVertexPositions[0] = bounds.min;
                boundsVertexPositions[1] = bounds.min;
                boundsVertexPositions[1].x = bounds.max.x;
                boundsVertexPositions[2] = bounds.min;
                boundsVertexPositions[2].y = bounds.max.y;
                boundsVertexPositions[3] = bounds.min;
                boundsVertexPositions[3].x = bounds.max.x;
                boundsVertexPositions[3].y = bounds.max.y;
                // 最大値
                boundsVertexPositions[4] = bounds.max;
                boundsVertexPositions[5] = bounds.max;
                boundsVertexPositions[5].x = bounds.min.x;
                boundsVertexPositions[6] = bounds.max;
                boundsVertexPositions[6].y = bounds.min.y;
                boundsVertexPositions[7] = bounds.max;
                boundsVertexPositions[7].x = bounds.min.x;
                boundsVertexPositions[7].y = bounds.min.y;

                // 8頂点をワールド空間に変換する。
                var aabbMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                var aabbMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
                for (int i = 0; i < boundsVertexPositions.Length; i++)
                {
                    Matrix4x4 vp = projMatrix * camera.worldToCameraMatrix ;
                    boundsVertexPositions[i] = mWorld.MultiplyPoint3x4(boundsVertexPositions[i]);
                    boundsVertexPositions[i] = vp.MultiplyPoint(boundsVertexPositions[i]);

                    aabbMin = Vector3.Max( Vector3.one * -1.0f, Vector3.Min(boundsVertexPositions[i], aabbMin) );
                    aabbMax = Vector3.Min( Vector3.one, Vector3.Max(boundsVertexPositions[i], aabbMax) );
                }
#endif
                // 拡大率を計算する。
                var halfSize = ( aabbMax - aabbMin ) * 0.5f;
                var scale = new Vector3(halfSize.x, halfSize.y, 1.0f);
                if( Mathf.Abs(scale.x) < 0.1f && Mathf.Abs(scale.y) < 0.1f )
                {
                    scale.x = 0.5f;
                    scale.y = 0.5f;
                }
                var posFromCenterNormalized = (aabbMin + aabbMax) * 0.5f;
                posFromCenterNormalized.z = 0.0f;
                // 最終描画用のワールド行列を計算する。
                var mWorldFinal = Matrix4x4.TRS(posFromCenterNormalized, Quaternion.identity, scale);
                // var mWorldFinal = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

                commandBuffer.SetRenderTarget(renderTextures.finalTexture);
                drawFinalList[litNo].Draw(
                    camera,
                    renderTextures.frontFaceDepthTexture,
                    renderTextures.backFaceDepthTexture,
                    commandBuffer,
                    volumeSpotLightList[litNo].volumeSpotLightData,
                    mWorldFinal
                );
            }
            
            commandBuffer.SetRenderTarget(cameraRenderTargetID);
            if (drawFinalList != null
                && drawFinalList.Count > 0
                && addCopyFullScreen != null)
            {
                addCopyFullScreen.Draw(
                    commandBuffer,
                    renderTextures.finalTexture,
                    BuiltinRenderTextureType.CameraTarget
                );
            }
            commandBuffer.ReleaseTemporaryRT(cameraTargetTextureID);
        }
    }

}