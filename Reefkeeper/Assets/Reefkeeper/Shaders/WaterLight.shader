Shader "Reefkeeper/Water Light"
{
    Properties { _BaseColor("Tint", Color)=(0.3,0.8,0.75,0.06) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial) float4 _BaseColor; CBUFFER_END
            struct A { float4 p:POSITION; float2 uv:TEXCOORD0; float4 c:COLOR; };
            struct V { float4 p:SV_POSITION; float2 uv:TEXCOORD0; float4 c:COLOR; };
            V vert(A a) { V o; o.p=TransformObjectToHClip(a.p.xyz); o.uv=a.uv; o.c=a.c; return o; }
            half4 frag(V i):SV_Target
            {
                float edge=pow(saturate(sin(i.uv.x*3.14159)),3);
                float fade=smoothstep(0,0.17,i.uv.y)*smoothstep(1,0.65,i.uv.y);
                return half4(_BaseColor.rgb*i.c.rgb,_BaseColor.a*i.c.a*edge*fade);
            }
            ENDHLSL
        }
    }
}
