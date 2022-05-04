#ifndef _DRAWVOLUMEMAP_HLSL_
#define _DRAWVOLUMEMAP_HLSL_

#pragma vertex vert
#pragma fragment frag
#pragma enable_d3d11_debug_symbols
#include "UnityCG.cginc"

sampler2D _CameraDepthTexture;
int volumeLightID;  // 描画するボリュームライトのID。

struct appdata
{
    float4 vertex : POSITION;

};

struct v2f
{
    float4 vertex : SV_POSITION;
    float4 posInProj : TEXCOORD0;
};



v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.posInProj = o.vertex;
    return o;
}

half frag(v2f i) : SV_Target
{
    float2 uv = i.posInProj.xy / i.posInProj.w;
    uv *= float2(0.5f, -0.5f);
    uv += 0.5f;
    half z = tex2D(_CameraDepthTexture, uv );
    z = max(i.vertex.z, z);
    return z;
}


#endif // _DRAWVOLUMEMAP_HLSL_  