Shader "Custom/PixelFontOutline_Glitch_Chromatic_Final"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _Color ("Main Color (Vertex)", Color) = (1,1,1,1)
        
        // --- UIGlowController ---
        [HDR] _FaceColor ("Face Color (Glow)", Color) = (1,1,1,1)
        // ------------------------

        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _AtlasWidth ("Atlas Width (Pixel)", Float) = 512.0
        [Range(0, 5)] _OutlineThickness ("Outline Thickness", Float) = 1.0

        // --- GLITCH & CHROMATIC AYARLARI ---
        [Header(Glitch Settings)]
        [Range(0, 1)] _GlitchStrength ("Glitch Strength", Float) = 0.0
        [Range(0, 50)] _GlitchFrequency ("Glitch Speed", Float) = 10.0
        [Range(1, 100)] _GlitchVertical ("Vertical Noise Density", Float) = 20.0
        // YENÝ: Renklerin ne kadar ayrýþacaðýný belirler
        [Range(0, 0.05)] _GlitchColorSplit ("Color Split Amount", Float) = 0.01 

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
            fixed4 _FaceColor; 

            float _AtlasWidth;
            float _OutlineThickness;
            float4 _ClipRect;

            // GLITCH DEÐÝÞKENLERÝ
            float _GlitchStrength;
            float _GlitchFrequency;
            float _GlitchVertical;
            float _GlitchColorSplit; // Yeni eklenen deðiþken

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

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
                // 1. GLITCH VE NOISE HESAPLAMASI
                // ------------------------------
                float2 glitchUV = IN.texcoord;
                float splitAmount = 0.0; // Renk ayrýþma miktarý

                if (_GlitchStrength > 0.0)
                {
                    float timeStep = floor(_Time.y * _GlitchFrequency);
                    float noiseVal = random(float2(timeStep, floor(IN.texcoord.y * _GlitchVertical)));

                    if (noiseVal > (1.0 - _GlitchStrength * 0.5)) 
                    {
                        // 1. Ana Koordinat Kaymasý (Mevcut özellik)
                        float shift = (random(float2(noiseVal, timeStep)) - 0.5) * 2.0; 
                        glitchUV.x += shift * _GlitchStrength * 0.1;

                        // 2. Renk Ayrýþmasý (Yeni Özellik)
                        // Glitch anýnda rastgele bir renk ayrýþmasý hesaplýyoruz.
                        // noiseVal'i kullanarak her satýrda farklý yöne ayrýlsýn istiyoruz.
                        splitAmount = shift * _GlitchColorSplit * _GlitchStrength;
                    }
                }
                
                // 2. TEXTURE OKUMALARI (CHROMATIC ABERRATION)
                // -------------------------------------------
                // RGB kanallarý için 3 ayrý UV koordinatý
                float2 uvR = glitchUV + float2(splitAmount, 0); // Kýrmýzý saða/sola
                float2 uvG = glitchUV;                          // Yeþil merkezde (orijinal glitch konumu)
                float2 uvB = glitchUV - float2(splitAmount, 0); // Mavi tam ters yöne
                
                // 3 kere texture okuyoruz (Maliyet artýþý, analizde açýklayacaðým)
                float alphaR = tex2D(_MainTex, uvR).a;
                float alphaG = tex2D(_MainTex, uvG).a;
                float alphaB = tex2D(_MainTex, uvB).a;

                // Maksimum alpha'yý buluyoruz ki maskeleme doðru olsun
                float maxAlpha = max(alphaR, max(alphaG, alphaB));

                fixed4 finalColor = fixed4(0,0,0,0);
                bool hasColor = false;

                // 3. ANA YAZI RENGÝNÝ OLUÞTURMA
                if (maxAlpha > 0.1)
                {
                    // Temel renk (Scriptten gelen FaceColor ve VertexColor)
                    fixed4 baseCol = IN.color * _FaceColor;

                    // RGB Kanallarýný birleþtiriyoruz:
                    // Kýrmýzý kanalýna -> R texture alphasýný atýyoruz
                    // Yeþil kanalýna  -> G texture alphasýný atýyoruz
                    // Mavi kanalýna   -> B texture alphasýný atýyoruz
                    finalColor.r = baseCol.r * alphaR;
                    finalColor.g = baseCol.g * alphaG;
                    finalColor.b = baseCol.b * alphaB;
                    
                    // Alpha, en görünür olan kanalýn alphasý olur.
                    finalColor.a = baseCol.a * maxAlpha;
                    
                    hasColor = true;
                }

                // 4. OUTLINE KONTROLÜ
                // Performans için Outline'a renk ayrýþmasý uygulamýyoruz.
                // Outline sadece "Merkez" (Yeþil) koordinata göre çizilir.
                // Bu sayede renkler outline'ýn dýþýna taþar (glitch hissini artýrýr).
                else if (_OutlineThickness > 0.0)
                {
                    float baseStep = 1.0 / _AtlasWidth;
                    float offset = baseStep * max(0.0, _OutlineThickness);
                    float alphaSum = 0.0;
                    
                    // Outline için hala 8 okuma yapýyoruz (glitchUV kullanarak)
                    alphaSum += tex2D(_MainTex, glitchUV + float2(offset, 0)).a;
                    alphaSum += tex2D(_MainTex, glitchUV - float2(offset, 0)).a;
                    alphaSum += tex2D(_MainTex, glitchUV + float2(0, offset)).a;
                    alphaSum += tex2D(_MainTex, glitchUV - float2(0, offset)).a;
                    
                    alphaSum += tex2D(_MainTex, glitchUV + float2(offset, offset)).a;
                    alphaSum += tex2D(_MainTex, glitchUV + float2(offset, -offset)).a;
                    alphaSum += tex2D(_MainTex, glitchUV + float2(-offset, offset)).a;
                    alphaSum += tex2D(_MainTex, glitchUV + float2(-offset, -offset)).a;

                    if (alphaSum > 0.1)
                    {
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