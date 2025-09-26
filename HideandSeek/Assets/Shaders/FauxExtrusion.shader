Shader "Universal Render Pipeline/UI/Faux Extrusion (Sprite)"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,1)
        _Cutoff  ("Alpha Cutoff", Range(0,1)) = 0.10
        _Thickness ("Thickness (world units)", Range(0, 1)) = 0.20
        _Layers    ("Depth Slices (≤37)", Range(2, 128)) = 32
        _BackFade  ("Back Fade", Range(0,1)) = 0.35
        _XYExpand ("Side Expand per Depth (0-0.5)", Range(0, 0.5)) = 0.05

        // --- Unity UI / Masking support ---
        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _UIMaskSoftnessX ("UI Softness X", Float) = 0
        _UIMaskSoftnessY ("UI Softness Y", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            // IMPORTANT: must be exactly UniversalRenderPipeline
            "RenderPipeline"="UniversalRenderPipeline"
            "CanUseSpriteAtlas"="True"
        }

        LOD 200
        Cull Off
        ZWrite Off                // UI should not write to depth
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            Name "FauxExtrusionUI"
            Tags{ "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 4.0                    // geometry shader required
            #pragma vertex   vert
            #pragma geometry geom
            #pragma fragment frag

            // UI feature toggles
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // NOTE: We avoid depending on UnityUI.hlsl to be robust across URP versions/packages.
            // Provide minimal local replacements used for RectMask2D clipping.
            // _ScreenParams is provided by Core.hlsl.
            inline float2 UI_GetPixelSize() { return 1.0 / _ScreenParams.xy; }
            inline half UI_Get2DClipping(float2 position, float4 clipRect)
            {
                // clipRect.xy = min, clipRect.zw = max
                float2 inside01 = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
                return (half)(inside01.x * inside01.y);
            }

            // Textures & samplers
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _Color;
            float  _Cutoff;
            float  _Thickness;
            float  _Layers;
            float  _BackFade;
            float  _XYExpand;

            float4 _ClipRect;        // set by Unity UI for RectMask2D
            float  _UIMaskSoftnessX;
            float  _UIMaskSoftnessY;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;      // tint from UI/SpriteRenderer
            };

            struct v2g
            {
                float4 localPos : POSITION; // object space
                float2 uv       : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos        : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float2 worldPos   : TEXCOORD1; // world position xy for UI clipping
                float  fade       : TEXCOORD2;
            };

            v2g vert(appdata v)
            {
                v2g o;
                o.localPos = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Each layer emits 3 vertices. 37 layers * 3 = 111 verts.
            // 144 verts * 7 scalars/vert = 1008 <= 1024 (D3D11 GS limit).
            [maxvertexcount(111)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                int layersReq = (int)round(_Layers);
                int layers = clamp(layersReq, 2, 37);
                float step = (layers <= 1) ? 0.0 : (_Thickness / (layers - 1));

                for (int i = 0; i < layers; i++)
                {
                    float zoff = -step * i;       // extend backward in object space
                    float t    = (float)i / (layers - 1);
                    float fade = lerp(1.0, 1.0 - _BackFade, t);

                    float4 p0 = IN[0].localPos; p0.z += zoff;
                    float4 p1 = IN[1].localPos; p1.z += zoff;
                    float4 p2 = IN[2].localPos; p2.z += zoff;

                    // Expand XY outward with depth to increase apparent thickness in silhouette
                    float expand = t * _XYExpand;
                    float2 scaleXY = 1.0 + expand;
                    p0.xy *= scaleXY;
                    p1.xy *= scaleXY;
                    p2.xy *= scaleXY;

                    g2f o;
                    o.fade = fade;

                    float3 wp0 = TransformObjectToWorld(p0.xyz);
                    float3 wp1 = TransformObjectToWorld(p1.xyz);
                    float3 wp2 = TransformObjectToWorld(p2.xyz);

                    o.pos = TransformWorldToHClip(wp0); o.worldPos = wp0.xy; o.uv = IN[0].uv; triStream.Append(o);
                    o.pos = TransformWorldToHClip(wp1); o.worldPos = wp1.xy; o.uv = IN[1].uv; triStream.Append(o);
                    o.pos = TransformWorldToHClip(wp2); o.worldPos = wp2.xy; o.uv = IN[2].uv; triStream.Append(o);

                    triStream.RestartStrip();
                }
            }

            float4 frag(g2f i) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // Alpha clip (cutout) first (optional)
                #if defined(UNITY_UI_ALPHACLIP)
                    clip(tex.a - _Cutoff);
                #endif

                // Apply RectMask2D / soft masks
                #if defined(UNITY_UI_CLIP_RECT)
                    float2 pixelSize = UI_GetPixelSize();
                    float2 softness = float2(_UIMaskSoftnessX, _UIMaskSoftnessY) * pixelSize;
                    float mask = UI_Get2DClipping(i.worldPos, _ClipRect);
                    if (softness.x > 0.0 || softness.y > 0.0)
                    {
                        float2 m = (abs(i.worldPos - _ClipRect.xy) - _ClipRect.zw);
                        m = 0.5 - m / max(softness, 1e-5);
                        mask = min(mask, saturate(min(m.x, m.y)));
                    }
                    tex.a *= mask;
                #endif

                float4 col = tex * _Color;
                col.rgb *= i.fade;
                return col;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
