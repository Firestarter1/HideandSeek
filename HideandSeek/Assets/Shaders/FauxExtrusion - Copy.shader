// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/FauxExtrusion_UI"
{
    Properties
    {
        // UI expects _MainTex to be marked PerRendererData so atlased sprites work correctly
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,1)
        _Cutoff  ("Alpha Cutoff", Range(0,1)) = 0.10

        // Extrusion controls (unchanged look)
        _Thickness ("Thickness (units)", Range(0, 1)) = 0.20
        _Layers    ("Depth Slices (≤32)", Range(2, 32)) = 32
        _BackFade  ("Back Fade", Range(0,1)) = 0.35
        _OverlayScreenStride ("Overlay Screen Stride", Range(0, 0.05)) = 0.0
        // Stencil / UI plumbing (lets Mask work; RectMask2D not enabled in this minimal build)
        [PerRendererData] _StencilComp ("Stencil Comparison", Float) = 8
        [PerRendererData] _Stencil     ("Stencil ID", Float)         = 0
        [PerRendererData] _StencilOp   ("Stencil Operation", Float)  = 0
        [PerRendererData] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [PerRendererData] _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        [PerRendererData] _ColorMask   ("Color Mask", Float)         = 15
        

    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }

        // Let UI Mask/MaskableGraphic stencil work
        Stencil
        {
            Ref   [_Stencil]
            Comp  [_StencilComp]
            Pass  [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        // UI render state
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma target 4.0                // geometry shader
            #pragma vertex   vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            float     _Cutoff;
            float _OverlayScreenStride;

            float     _Thickness;
            float     _Layers;
            float     _BackFade;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2g
            {
                float4 localPos : TEXCOORD0;
                float2 uv       : TEXCOORD1;
                fixed4 color    : COLOR;
            };

            // Keep varyings lean for GS budget: pos(4) + uv(2) + color(4) = 10 scalars
            struct g2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                fixed4 color : COLOR;   // alpha carries per-layer fade
            };

            v2g vert(appdata_t v)
            {
                v2g o;
                o.localPos = v.vertex;
                o.uv       = TRANSFORM_TEX(v.uv, _MainTex);
                o.color    = v.color * _Color; // apply UI tint here
                return o;
            }

            // 32 layers → 96 verts; 96 * 10 = 960 <= 1024 (D3D GS cap)
            [maxvertexcount(96)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                int layersReq = (int)round(_Layers);
                int layers    = clamp(layersReq, 2, 32);

                float step = (layers <= 1) ? 0.0 : (_Thickness / (layers - 1));

                for (int i = 0; i < layers; i++)
                {
                    float zoff = -step * i;                      // extrude “backwards”
                    float t    = (float)i / (layers - 1);
                    float fade = lerp(1.0, _BackFade, t);         // deeper = darker

                    [unroll] for (int v = 0; v < 3; v++)
                    {
                        float3 lp = float3(IN[v].localPos.xy, IN[v].localPos.z + zoff);
                        float4 obj = float4(lp, 1);

                        g2f o;
                        o.pos   = UnityObjectToClipPos(obj);
                        if (_OverlayScreenStride > 0)
                        {
                        // How a unit step along local X maps into clip-space XY:
                        float2 rightClip = UnityObjectToClipPos(float4(1,0,0,0)).xy;
                        float  m = max(1e-6, length(rightClip));
                        rightClip /= m;

                        // Move this slice sideways in clip-space proportional to its Z "depth"
                        float stride = zoff * _OverlayScreenStride;   // zoff is negative for back slices
                        o.pos.xy += rightClip * stride;
                        }
                        o.uv    = IN[v].uv;
                        o.color = IN[v].color;
                        o.color.a *= fade; // pack fade into alpha to avoid extra varyings
                        triStream.Append(o);
                    }
                    triStream.RestartStrip();
                }
            }

            fixed4 frag(g2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Hard cutout (same look as your SpriteRenderer version)
                clip(tex.a - _Cutoff);

                // Multiply brightness by per-layer fade (carried in color.a)
                // and apply UI tint (already in color.rgb)
                fixed3 rgb = tex.rgb * i.color.rgb * i.color.a;

                // Opaque inside the cutout, like your original
                return fixed4(rgb, 1);
            }
            ENDCG
        }
    }

    Fallback Off
}
