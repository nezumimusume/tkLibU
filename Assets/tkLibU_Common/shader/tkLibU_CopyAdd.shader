Shader "tkLibU/CopyAdd"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend One One
            Cull Off
            ZWrite Off
            ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
            #include "UnityCG.cginc"
            #include "tkLibU_Util.hlsl"

            sampler2D srcTexture; // 
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 posInProj : TEXCOORD0;   // 射影空間の座標。
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.posInProj = o.vertex;
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {    
                // 各種UV座標の計算。
                float2 uv = CalcUVCoordFromClip(i.posInProj);
                return tex2D(srcTexture, uv);
            }
            ENDCG
        }
    }
}
