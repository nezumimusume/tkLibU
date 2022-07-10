Shader "tkPlRef/Reciever"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma multi_compile _ TK_DEFERRED_PASS
            #pragma vertex vert
            #pragma fragment frag
           
            #include "UnityCG.cginc"
            #include "Assets/tkLibU_Common/shader/tkLibU_Util.hlsl"

            struct VSInput
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VSOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 posInProj : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _planeReflectionTex;
            
            VSOutput vert( VSInput vsIn )
            {
                VSOutput vsOut = (VSOutput)0;
                vsOut.pos = UnityObjectToClipPos(vsIn.pos);
                vsOut.uv = vsIn.uv;
                vsOut.posInProj = vsOut.pos;
                return vsOut;
            }
            fixed4 frag(VSOutput vsOut) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, vsOut.uv);
                float2 uv = CalcUVCoordFromClip(vsOut.posInProj);
                fixed4 refCol = tex2D(_planeReflectionTex, uv);
                fixed4 finalCol = lerp( col, refCol, 0.5f);
                return finalCol;
            }
            ENDCG
        }
    }
}
