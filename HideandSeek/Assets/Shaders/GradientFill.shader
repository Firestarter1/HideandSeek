Shader "UI/Gradient Fill (Mask)"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Color   ("Color", Color) = (1,1,1,1)

        _FillMap ("Fill Gradient Map", 2D) = "gray" {}
        _Fill    ("Fill (0..1)", Range(0,1)) = 0
        _Edge    ("Edge Softness", Range(0,0.2)) = 0.02
        _Invert  ("Invert", Float) = 0 
        _UseLum  ("Use Luminance", Float) = 1 

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil    ("Stencil ID", Float) = 0
        _StencilOp  ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        _ClipRect ("Clip Rect", Vector) = ( -32767, -32767, 32767, 32767 )
        _UIMaskSoftnessX ("Mask Softness X", Float) = 0
        _UIMaskSoftnessY ("Mask Softness Y", Float) = 0
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI-GradientFill"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _FillMap; float4 _FillMap_ST;
            fixed4 _Color;

            float _Fill;
            float _Edge;
            float _Invert;
            float _UseLum;

            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            float _UseUIAlphaClip;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1; 
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uvFill : TEXCOORD2;
                float4 worldPos : TEXCOORD1;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uvFill = TRANSFORM_TEX(v.texcoord, _FillMap);
                o.color = v.color * _Color;
                o.worldPos = v.vertex; 
                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.uv) * IN.color;

                fixed4 g = tex2D(_FillMap, IN.uvFill);

                half grad = (_UseLum > 0.5) ? dot(g.rgb, half3(0.2126h, 0.7152h, 0.0722h)) : g.r;
                if (_Invert > 0.5) grad = 1.0h - grad;

                half aMask = 1.0h - smoothstep(_Fill - _Edge, _Fill + _Edge, grad);

                #ifdef UNITY_UI_CLIP_RECT
                half2 maskSoftness = half2(_UIMaskSoftnessX, _UIMaskSoftnessY);
                aMask *= UnityGet2DClipping(IN.worldPos.xy, _ClipRect) * UnityGet2DClippingSoft(IN.worldPos.xy, _ClipRect, maskSoftness);
                #endif

                col.a *= saturate(aMask);

                #ifdef UNITY_UI_ALPHACLIP
                if (_UseUIAlphaClip > 0.5 && col.a < 0.001) discard;
                #endif

                return col;
            }
            ENDCG
        }
    }
}
