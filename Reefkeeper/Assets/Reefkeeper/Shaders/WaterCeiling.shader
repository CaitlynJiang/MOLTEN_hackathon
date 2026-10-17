Shader "Reefkeeper/Water Ceiling"
{
    Properties { _BaseColor("Tint", Color)=(0.12,0.63,0.66,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial) float4 _BaseColor; CBUFFER_END
            struct A { float4 p:POSITION; };
            struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; float fog:TEXCOORD1; };
            V vert(A a) { V o; o.world=TransformObjectToWorld(a.p.xyz); o.p=TransformWorldToHClip(o.world); o.fog=ComputeFogFactor(o.p.z); return o; }
            half4 frag(V i):SV_Target
            {
                float2 p=i.world.xz;
                float wave=sin(p.x*1.3+sin(p.y*0.65+_Time.y*0.2)*2+_Time.y*0.5)
                    *sin(p.y*1.1+sin(p.x*0.5-_Time.y*0.15));
                float shimmer=pow(saturate(wave),5);
                float sun=pow(saturate(dot(normalize(i.world-_WorldSpaceCameraPos),normalize(float3(-0.3,0.9,0.2)))),22);
                float3 col=_BaseColor.rgb*(0.8+wave*0.12)+shimmer*float3(0.16,0.25,0.34)+sun*float3(0.62,0.79,1.0);
                return half4(MixFog(col,i.fog*0.5),1);
            }
            ENDHLSL
        }
    }
}
