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
                float4 pos : POSITION;      // 頂点座標
                float2 uv : TEXCOORD0;      // UV座標
                float3 normal : NORMAL;     // 法線
                float3 tangent : TANGENT;   // 節ベクトル
            };

            struct VSOutput
            {
                float4 pos : SV_POSITION;   // 座標
                float2 uv : TEXCOORD0;      // UV座標
                float3 normal : NORMAL;     // 法線
                float3 tangent : TANGENT;   // 節ベクトル
                float biNormal : BINORMAL; // 従ベクトル
            };

            // アルベドマップ。
            sampler2D _MainTex;
            float4 _MainTex_ST;

            // 法線マップ。
            sampler2D _NormalMap;
            float4 _NormalMap_ST;

            // メタリックスムースマップ。Unityのメタリックワークフローに準拠。
            sampler2D _MetallicSmoothMap;
            float4 _MetallicSmoothMap_ST;
            
            VSOutput vert( VSInput vsIn )
            {
                VSOutput vsOut = (VSOutput)0;
                vsOut.pos = UnityObjectToClipPos(vsIn.pos);
                vsOut.uv = vsIn.uv;
                vsOut.normal = mul(unity_ObjectToWorld, vsIn.normal);
                vsOut.tangent = mul(unity_ObjectToWorld, vsIn.tangent);;
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
