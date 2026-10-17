Shader "Reefkeeper/Pickup Outline"
{
    Properties
    {
        [HDR] _OutlineColor("Outline color", Color) = (1.6,1.6,1.6,1)
        _OutlineWidth("World width", Range(0.001,0.015)) = 0.004
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+10" }
        Pass
        {
            Cull Front
            ZWrite Off
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END
            struct A { float4 position:POSITION; float3 normal:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 position:SV_POSITION; UNITY_VERTEX_OUTPUT_STEREO };
            V vert(A input)
            {
                V output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float3 world=TransformObjectToWorld(input.position.xyz);
                world+=TransformObjectToWorldNormal(input.normal)*_OutlineWidth;
                output.position=TransformWorldToHClip(world);
                return output;
            }
            half4 frag(V input):SV_Target { return _OutlineColor; }
            ENDHLSL
        }
    }
}
