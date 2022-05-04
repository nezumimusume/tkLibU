Shader "tkVlit/DrawBackFaceDepth"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        
        Pass
        {
            Cull Front
            
            CGPROGRAM

            #include "tkVlit_DrawDepth.hlsl"

            ENDCG
        }
    }
}
