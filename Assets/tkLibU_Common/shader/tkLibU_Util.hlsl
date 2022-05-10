#ifndef _TKLIBU_UTIL_HLSL_
#define _TKLIBU_UTIL_HLSL_

// クリップ空間からUV座標を計算する。
// プラットフォーム間の際を吸収したUV座標を計算します。
inline float2 CalcUVCoordFromClip(float4 coordInClipSpace )
{
    float2 uv = coordInClipSpace.xy / coordInClipSpace.w;
#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLCORE) || defined(SHADER_API_D3D11_9X) || defined(TK_DEFERRED_PASS)
    uv *= float2(0.5f, -0.5f);
#else
    uv *= float2(0.5f, 0.5f);
#endif
    uv += 0.5f;

    return uv;
}
// DirectX系のクリップ空間からのUV座標を計算する。
// 深度テクスチャなどのユーザー定義のレンダリングテクスチャ以外はDirectX系のクリップ空間に変換されているっぽい？
inline float2 CalcUVCoordFromClipInDxSpace(float4 coordInClipSpace)
{
    float2 uv = coordInClipSpace.xy / coordInClipSpace.w;
    uv *= float2(0.5f, -0.5f);
    uv += 0.5f;
    return uv;
}

#endif // _TKLIBU_UTIL_HLSL_