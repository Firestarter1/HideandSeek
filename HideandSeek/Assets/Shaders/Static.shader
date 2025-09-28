Shader "UI/URP/Static"
{
    Properties
    {
        [PerRendererData]_MainTex ("(Ignored) Texture", 2D) = "white" {}
        _Color      ("Tint", Color) = (1,1,1,1)
        _Opacity    ("Opacity", Range(0,1)) = 0.35

        _GrainSize  ("Grain Size (px)", Range(1,16)) = 2
        _NoiseSpeed ("Noise Speed (Hz)", Range(0,60)) = 12

        _ScanlineIntensity ("Scanline Intensity", Range(0,1)) = 0.35
        _ScanlineCount     ("Scanlines (per screen)", Range(64,2000)) = 1080

        _Seed ("Seed", Float) = 1.0
        _UnscaledTime ("(Driven by script)", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil     ("Stencil ID", Float) = 0
        _StencilOp   ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="False"
            "UniversalMaterialType"="Unlit"
            "CanvasOverlay"="True"
        }

        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex; 
            fixed4 _Color;
            float  _Opacity;

            float  _GrainSize;
            float  _NoiseSpeed;

            float  _ScanlineIntensity;
            float  _ScanlineCount;

            float  _Seed;
            float  _UnscaledTime;
            float4 _ClipRect;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f {
                float4 pos      : SV_POSITION;
                fixed4 color    : COLOR;
                float4 worldPos : TEXCOORD0;
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.color    = v.color * _Color;
                o.worldPos = v.vertex;
                return o;
            }

            float hash12(float2 p) {
                p = frac(p * 0.1031);
                p += dot(p, p.yx + 33.33);
                return frac(p.x * p.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 ndc  = i.pos.xy / i.pos.w;         
                float2 uv01 = ndc * 0.5 + 0.5;            
                float2 pix  = uv01 * _ScreenParams.xy;    

                float t = (_UnscaledTime > 0) ? _UnscaledTime : _Time.y;

                float g = max(1.0, _GrainSize);
                float2 cell = floor(pix / g);

                float anim = floor(t * _NoiseSpeed);
                float n = hash12(cell + anim + _Seed);

                float scPhase = 3.14159265 * (pix.y * (_ScanlineCount / _ScreenParams.y));
                float scan = 0.5 + 0.5 * sin(scPhase);
                n = lerp(n, n * scan, _ScanlineIntensity);

                fixed4 col;
                col.rgb = n.xxx * i.color.rgb;   
                col.a   = _Opacity * i.color.a;  

                #ifdef UNITY_UI_CLIP_RECT
                    col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                    clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}
