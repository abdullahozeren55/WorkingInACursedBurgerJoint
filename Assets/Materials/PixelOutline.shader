Shader "Custom/PixelFontOutline_AnimSupport_Final"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _Color ("Main Color (Vertex)", Color) = (1,1,1,1)
        
        // --- UIGlowController ÝÇÝN GEREKLÝ ---
        // Scriptin aradýðý _FaceColor özelliði. Default beyaz yaptýk ki script çalýþmazsa yazý normal görünsün.
        [HDR] _FaceColor ("Face Color (Glow)", Color) = (1,1,1,1)
        // -------------------------------------

        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _AtlasWidth ("Atlas Width (Pixel)", Float) = 512.0
        [Range(0, 5)] _OutlineThickness ("Outline Thickness", Float) = 1.0

        // --- MASK FIX STANDARTLARI ---
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
            
            // Scriptten gelen Glow Rengi buraya düþecek
            fixed4 _FaceColor; 

            float _AtlasWidth;
            float _OutlineThickness;
            float4 _ClipRect;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                // Vertex color (Text Animator vs) ile Main Color çarpýmý
                OUT.color = IN.color * _Color; 
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float baseStep = 1.0 / _AtlasWidth;
                float offset = baseStep * max(0.0, _OutlineThickness);
                float mainAlpha = tex2D(_MainTex, IN.texcoord).a;

                fixed4 finalColor = fixed4(0,0,0,0);
                bool hasColor = false;

                // 1. ANA YAZI
                if (mainAlpha > 0.1)
                {
                    // BURASI DEÐÝÞTÝ:
                    // IN.color (Vertex+Main) * _FaceColor (Scriptten gelen Glow)
                    // Böylece script rengi parlattýðýnda yazý da parlayacak.
                    finalColor = IN.color * _FaceColor;
                    
                    // Alpha'yý texture'dan alýp uyguluyoruz
                    finalColor.a *= mainAlpha;
                    
                    hasColor = true;
                }
                // 2. OUTLINE KONTROLÜ
                else if (_OutlineThickness > 0.0)
                {
                    float alphaSum = 0.0;
                    
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(offset, 0)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord - float2(offset, 0)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(0, offset)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord - float2(0, offset)).a;
                    
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(offset, offset)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(offset, -offset)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(-offset, offset)).a;
                    alphaSum += tex2D(_MainTex, IN.texcoord + float2(-offset, -offset)).a;

                    if (alphaSum > 0.1)
                    {
                        // Outline rengi scriptten etkilenmesin istiyorsan böyle kalsýn.
                        // Etkilensin istersen _OutlineColor * _FaceColor yapabilirsin.
                        // Þimdilik sadece ana yazý parlasýn diye böyle býrakýyorum.
                        finalColor = fixed4(_OutlineColor.rgb, _OutlineColor.a * IN.color.a);
                        hasColor = true;
                    }
                }

                if (hasColor)
                {
                    finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    return finalColor;
                }

                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}