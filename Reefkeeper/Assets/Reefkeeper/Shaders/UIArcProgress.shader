Shader "Reefkeeper/UI Arc Progress"
{
    Properties
    {
        [PerRendererData] _MainTex("Arc sprite", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Progress("Progress", Range(0,1)) = 0
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="False" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct Input { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Output { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float4 local : TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
            sampler2D _MainTex;
            fixed4 _Color, _TextureSampleAdd;
            float4 _ClipRect;
            float _Progress;
            Output vert(Input v)
            {
                Output o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.local = v.vertex; o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; o.color = v.color * _Color; return o;
            }
            fixed4 frag(Output i) : SV_Target
            {
                fixed4 color = (tex2D(_MainTex, i.uv) + _TextureSampleAdd) * i.color;
                // Both original 512px canvases are untrimmed. The arc runs from -58 to +58 degrees.
                float2 p = (i.uv - 0.5) * 512;
                float endAngle = radians(-58 + 116 * saturate(_Progress));
                float angle = atan2(p.y, p.x);
                float angleAA = max(fwidth(angle), 0.0001);
                float sector = 1 - smoothstep(endAngle - angleAA, endAngle + angleAA, angle);
                // A moving 5.5px-radius cap matches the original 11px white stroke.
                float2 endPoint = 180 * float2(cos(endAngle), sin(endAngle));
                float capDistance = length(p - endPoint);
                float capAA = max(fwidth(capDistance), 0.5);
                float cap = 1 - smoothstep(5.5 - capAA, 5.5 + capAA, capDistance);
                color.a *= _Progress <= 0 ? 0 : max(sector, cap);
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.local.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif
                return color;
            }
            ENDCG
        }
    }
}
