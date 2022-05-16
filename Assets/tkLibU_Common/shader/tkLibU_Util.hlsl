#ifndef _TKLIBU_UTIL_HLSL_
#define _TKLIBU_UTIL_HLSL_

// クリップ空間からUV座標を計算する。
// プラットフォーム間の際を吸収したUV座標を計算します。
inline float2 CalcUVCoordFromClip(float4 coordInClipSpace )
{
    float2 uv = coordInClipSpace.xy / coordInClipSpace.w;
    // todo androidのディファードのパスで要動作確認。
    uv *= float2(0.5f, 0.5f * _ProjectionParams.x);

    uv += 0.5f;

    return uv;
}

// クリップ空間からUV座標を計算する。
// この関数は上下反転していないテクスチャで利用する。
inline float2 CalcUVCoordFromClip_NoVFlipTexture(float4 coordInClipSpace)
{
    float2 uv = coordInClipSpace.xy / coordInClipSpace.w;
    uv *= float2(0.5f, -0.5f);
    uv += 0.5f;

    return uv;
}

#endif // _TKLIBU_UTIL_HLSL_