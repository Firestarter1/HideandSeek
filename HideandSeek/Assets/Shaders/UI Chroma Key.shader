Shader "UI/URP/ChromaKey RawImage"
{
    Properties
    {
        [PerRendererData]_MainTex ("Texture", 2D) = "white" {}
        _Color      ("Tint", Color) = (1,1,1,1)

        _KeyColor   ("Key Color", Color) = (0,1,0,1)   
        _Tolerance  ("Tolerance", Range(0,1)) = 0.15    
        _Softness   ("Softness",  Range(0,1)) = 0.05    
        _Despill    ("Despill",   Range(0,1)) = 0.35    

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil     ("Stencil ID", Float) = 0
        _StencilOp   ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask", Float) = 255
        _ColorMask   ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _ClipRect    ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
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
            float4    _MainTex_TexelSize;
            fixed4    _Color;

            fixed4    _KeyColor;
            float     _Tolerance;
            float     _Softness;
            float     _Despill;
            float4    _ClipRect;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos          : SV_POSITION;
                float2 uv           : TEXCOORD0;
                fixed4 color        : COLOR;
                float4 worldPos     : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.uv       = v.uv;
                o.color    = v.color * _Color;
                o.worldPos = v.vertex; 
                return o;
            }

            float3 RGB2YCbCr(float3 c)
            {
                float y  = dot(c, float3(0.2989, 0.5870, 0.1140));
                float cb = (c.b - y) * 0.564;
                float cr = (c.r - y) * 0.713;
                return float3(y, cb, cr);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                float3 ycc      = RGB2YCbCr(col.rgb);
                float3 keyYcc   = RGB2YCbCr(_KeyColor.rgb);

                float chromaDist = distance(ycc.yz, keyYcc.yz);

                float keep = 1.0 - smoothstep(_Tolerance - _Softness, _Tolerance + _Softness, chromaDist);

                float alphaFactor = 1.0 - keep;

                float nearKey = saturate(1.0 - chromaDist / max(1e-5, _Tolerance));
                float keyProj = saturate(dot(col.rgb, _KeyColor.rgb) / max(1e-5, dot(_KeyColor.rgb, _KeyColor.rgb)));
                float3 despilled = col.rgb - _KeyColor.rgb * keyProj * _Despill * nearKey;
                col.rgb = lerp(col.rgb, despilled, _Despill);

                col.a *= alphaFactor;

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
