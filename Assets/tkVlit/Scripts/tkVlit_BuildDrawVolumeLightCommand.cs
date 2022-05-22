using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolumeLight
{
    /// <summary>
    /// ボリュームライト描画ためのコマンドを構築する処理。
    /// </summary>
    public class tkVlit_BuildDrawVolumeLightCommand
    {
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
                // todo プラットフォームによってはクリアする値を変更する必要があるかも。
                commandBuffer.ClearRenderTarget(true, true, Color.white);
                commandBuffer.DrawMesh(
                    drawFrontMeshFilterList[litNo].sharedMesh,
                    mWorld,
                    drawFrontFaceMaterialList[litNo]
                );
                
                commandBuffer.SetRenderTarget(renderTextures.finalTexture);
                
                drawFinalList[litNo].Draw(
                    camera,
                    renderTextures.frontFaceDepthTexture,
                    renderTextures.backFaceDepthTexture,
                    commandBuffer,
                    volumeSpotLightList[litNo].volumeSpotLightData
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