Shader "Custom/PixelFontOutline_AnimSupport_Fixed"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _Color ("Text Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _AtlasWidth ("Atlas Width (Pixel)", Float) = 512.0

        // --- MASK FIX BAÞLANGICI ---
        // Unity UI Masking için gerekli standart özellikler
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        // --- MASK FIX BÝTÝÞÝ ---
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        // --- STENCIL BLOK (Maskeleme Komutlarý) ---
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        // -----------------------------------------

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc" // UI Clip fonksiyonlarý için gerekli olabilir

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _AtlasWidth;
            // Masking için clip rect deðiþkeni
            float4 _ClipRect;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color; 
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float step = 1.0 / _AtlasWidth;

                // Texture Alpha deðerini al
                float mainAlpha = tex2D(_MainTex, IN.texcoord).a;

                // Çýktý rengi için deðiþken
                fixed4 finalColor = fixed4(0,0,0,0);
                bool hasColor = false;

                // 1. ANA YAZI
                if (mainAlpha > 0.1)
                {
                    finalColor = fixed4(IN.color.rgb, mainAlpha * IN.color.a);
                    hasColor = true;
                }
                // 2. OUTLINE KONTROLÜ
                else
                {
                    float alphaSum = 0.0;
                    
                    // 4 Yön + Çaprazlar
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(step, 0)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord - float2(step, 0)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(0, step)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord - float2(0, step)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(step, step)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(step, -step)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(-step, step)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(-step, -step)).a;

                    if (alphaSum > 0.1)
                    {
                        finalColor = fixed4(_OutlineColor.rgb, _OutlineColor.a * IN.color.a);
                        hasColor = true;
                    }
                }

                if (hasColor)
                {
                    // --- MASK CHECK ---
                    // RectMask2D kullanýrsan diye bunu da ekledim garanti olsun.
                    // Normal Mask için üstteki STENCIL bloðu çalýþacak.
                    finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    return finalColor;
                }

                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}