Shader "Reefkeeper/Water Ceiling"
{
    Properties
    {
        _BaseColor("Deep surface blue", Color)=(0.06,0.36,0.56,1)
        _TransmissionColor("Daylight through water", Color)=(0.28,0.59,0.78,1)
        [HDR] _ReflectionColor("Broken sky reflections", Color)=(0.85,1.05,1.15,1)
        _ReflectionStrength("Reflection strength", Range(0,3))=1.4
        _WaveScale("Ripple scale", Range(0.2,3))=1.15
        _SunDirection("Sun direction", Vector)=(-0.3,0.9,0.2,0)
    }
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
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor, _TransmissionColor, _ReflectionColor, _SunDirection;
                float _ReflectionStrength, _WaveScale;
            CBUFFER_END
            struct A { float4 p:POSITION; };
            struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; };
            V vert(A a) { V o; o.world=TransformObjectToWorld(a.p.xyz); o.p=TransformWorldToHClip(o.world); return o; }
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float Noise(float2 p)
            {
                float2 cell=floor(p), f=frac(p);
                f=f*f*(3-2*f);
                return lerp(lerp(Hash(cell),Hash(cell+float2(1,0)),f.x),
                    lerp(Hash(cell+float2(0,1)),Hash(cell+1),f.x),f.y);
            }
            float Ripples(float2 p)
            {
                return Noise(p)*0.55+Noise(p*2.07+11.4)*0.29+Noise(p*4.13-7.3)*0.16;
            }
            half4 frag(V i):SV_Target
            {
                float2 p=i.world.xz*float2(0.62,1.15)*_WaveScale;
                float t=_Time.y*0.045;
                float2 warp=float2(Ripples(p*0.48+float2(t,0)),Ripples(p*0.48+17.2-float2(0,t)))-0.5;
                float reflection=Ripples(p+warp*2.1+float2(t,-t*0.7));
                float3 ray=normalize(i.world-_WorldSpaceCameraPos+float3(warp.x,0,warp.y)*1.4);
                float overhead=smoothstep(0.18,0.7,ray.y);
                float brokenLight=smoothstep(0.48,0.68,reflection);
                float sun=pow(saturate(dot(ray,normalize(_SunDirection.xyz))),38);
                // An art-directed transmitted sky with moving, irregular silver patches.
                float3 col=lerp(_BaseColor.rgb,_TransmissionColor.rgb,0.30+overhead*0.55+reflection*0.15);
                col+=_ReflectionColor.rgb*brokenLight*_ReflectionStrength*(0.25+overhead*0.75);
                col+=sun*float3(1.6,1.48,1.22);
                // Evaluate fog per pixel on the broad ceiling, preserving nearby
                // reflections while fading its distant edge into the water.
                // Scaling a linear fog factor toward zero would add more fog.
                float fog=ComputeFogFactor(TransformWorldToHClip(i.world).z);
                return half4(MixFog(col,fog),1);
            }
            ENDHLSL
        }
    }
}
