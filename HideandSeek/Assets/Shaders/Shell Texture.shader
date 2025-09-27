Shader "UI/ShellSprite-URP-AlphaClip"
{
    Properties
    {
        _MainTex("Sprite", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _AlphaCut("Alpha Cutoff", Range(0,1)) = 0.1

        // UI stencil / clipping
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_CLIP_RECT)] _UseClipRect ("Use RectMask2D clipping", Float) = 1
        _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
        _UIMaskSoftnessX ("Mask Softness X", Float) = 0
        _UIMaskSoftnessY ("Mask Softness Y", Float) = 0
    }

    SubShader
    {
        Tags{
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
            "RenderPipeline"="UniversalPipeline"
        }

        Stencil{
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        // Keep ZWrite Off to match UI sorting; cull off so shells show from the side
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local __ UNITY_UI_CLIP_RECT
            #pragma multi_compile_fragment _ UNITY_UI_ALPHACLIP
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
                float2 pos2D  : TEXCOORD1; // screen-space UI pos for clip rect
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 posCS : SV_POSITION;
                float2 uv    : TEXCOORD0;
                float4 color : COLOR;
                float2 pos2D : TEXCOORD1;
                float3 wpos  : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _AlphaCut;
                float _UseClipRect;
                float4 _ClipRect;
                float _UIMaskSoftnessX;
                float _UIMaskSoftnessY;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.posCS = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color * _Color;
                o.pos2D = v.pos2D;
                o.wpos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            float UnityGet2DClipping(float2 position, float4 clipRect)
            {
                float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
                return inside.x * inside.y;
            }

            float ComputeSoftMask(float2 pos, float4 clipRect, float sx, float sy)
            {
                float2 rectMin = clipRect.xy;
                float2 rectMax = clipRect.zw;
                float2 deltaToMin = pos - rectMin;
                float2 deltaToMax = rectMax - pos;
                float2 softness = float2(max(0.001, sx), max(0.001, sy));
                float2 t = min(min(deltaToMin / softness, deltaToMax / softness), 1.0);
                return saturate(min(t.x, t.y));
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // UI clip rect support
                if (_UseClipRect > 0.5)
                {
                    float mask = UnityGet2DClipping(i.pos2D, _ClipRect);
                    if (mask <= 0.0) discard;
                    float soft = ComputeSoftMask(i.pos2D, _ClipRect, _UIMaskSoftnessX, _UIMaskSoftnessY);
                    c.a *= soft;
                }

                // Alpha clip to get a crisp silhouette per shell
                if (c.a < _AlphaCut) discard;

                c *= i.color;
                return c;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
