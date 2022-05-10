Shader "tkVlit/DrawBackFaceDepth"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Back

            CGPROGRAM
            #pragma multi_compile _ TK_DEFERRED_PASS
            #include "tkVlit_DrawDepth.hlsl"

            ENDCG
        }
    }
}
