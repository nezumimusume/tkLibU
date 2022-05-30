Shader "tkPlRef/DrawCaster"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        
        Pass
        {
            Cull Front
            
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            VSOutput vert( VSInput vsIn )
            {
                VSOutput vsOut = (VSOutput)0;
                vsOut.pos = UnityObjectToClipPos(vsIn.pos);
                vsOut.uv = vsIn.uv;
                return vsOut;
            }
            fixed4 frag(VSOutput vsOut) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, vsOut.uv);
                return col;
            }
            ENDCG
        }
    }
}
