Shader "Reefkeeper/Reef Surface"
{
    Properties
    {
        _BaseColor ("Color", Color) = (0.6,0.7,0.6,1)
        _Caustics ("Caustic strength", Range(0,2)) = 0.65
        _Sand ("Sand ripples", Range(0,1)) = 0
        _Sway ("Plant sway", Range(0,1)) = 0
        _Cull ("Cull", Float) = 2
        [HideInInspector] _BaseMap ("Texture", 2D) = "white" {}
        [HideInInspector] _Cutoff ("Cutoff", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Caustics, _Sand, _Sway, _Cull, _Cutoff;
                float4 _BaseMap_ST;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float fog:TEXCOORD2; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 p = v.positionOS.xyz;
                p.x += sin(_Time.y * 0.8 + TransformObjectToWorld(p).x * 0.6 + p.y) * _Sway * max(0,p.y) * 0.12;
                VertexPositionInputs pos = GetVertexPositionInputs(p);
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.fog = ComputeFogFactor(pos.positionCS.z);
                return o;
            }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7))) * 43758.5453); }
            float cells(float2 p)
            {
                float2 baseCell = floor(p), f = frac(p);
                float nearest = 9, second = 9;
                [unroll] for(int y=-1; y<=1; y++)
                [unroll] for(int x=-1; x<=1; x++)
                {
                    float2 cell=float2(x,y);
                    float2 random=float2(hash(baseCell+cell),hash(baseCell+cell+19.3));
                    float2 offset=cell+0.5+0.3*sin(_Time.y*0.6+6.283*random)-f;
                    float d=dot(offset,offset);
                    if(d<nearest) { second=nearest; nearest=d; }
                    else second=min(second,d);
                }
                return 1-smoothstep(0.015,0.12,second-nearest);
            }
            half4 frag(Varyings i):SV_Target
            {
                float3 n=normalize(i.normalWS);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float lit=saturate(dot(n,sun.direction));
                float ripple=sin(i.positionWS.x*17+sin(i.positionWS.z*1.3)*2) * 0.035 * _Sand;
                float variation=0.92+0.08*sin(i.positionWS.x*2.8+i.positionWS.y*5.2)*sin(i.positionWS.z*3.4);
                float3 baseColor=_BaseColor.rgb*(variation+ripple);
                float3 ambient=float3(0.19,0.38,0.39) + saturate(n.y)*float3(0.07,0.12,0.1);
                float3 col=baseColor*(ambient+sun.color*lit*sun.shadowAttenuation*0.68);
                float2 uv=i.positionWS.xz*0.72 + i.positionWS.y*0.08;
                float caustic=cells(uv+float2(_Time.y*0.035,-_Time.y*0.022));
                caustic *= saturate(n.y*0.8+0.2)*sun.shadowAttenuation;
                col += float3(0.45,0.85,0.72)*caustic*_Caustics*0.44;
                col=lerp(col,col*float3(0.56,0.89,1),saturate(distance(_WorldSpaceCameraPos,i.positionWS)/50)*0.45);
                return half4(MixFog(col,i.fog),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
