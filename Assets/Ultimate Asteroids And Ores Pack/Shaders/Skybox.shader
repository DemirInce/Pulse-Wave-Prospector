Shader "Custom/TriColorSpaceSkyboxWithStars"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.1, 0.1, 0.2, 1)
        _MidColor ("Middle Color", Color) = (0.3, 0.2, 0.5, 1)
        _BottomColor ("Bottom Color", Color) = (0.05, 0.05, 0.1, 1)
        _MidPoint ("Middle Point", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0.01, 1)) = 0.2
        
        _StarDensity ("Star Density", Range(1, 50)) = 15
        _StarSize ("Star Size", Range(50, 500)) = 200
        _StarBrightness ("Star Brightness", Range(0, 2)) = 1
        _StarTwinkleSpeed ("Twinkle Speed", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _TopColor;
            float4 _MidColor;
            float4 _BottomColor;
            float _MidPoint;
            float _Smoothness;
            float _StarDensity;
            float _StarSize;
            float _StarBrightness;
            float _StarTwinkleSpeed;

            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float stars(float2 uv)
            {
                uv *= _StarDensity;
                float2 gv = frac(uv) - 0.5;
                float2 id = floor(uv);
                
                float minDist = 1.0;
                
                for(float y = -1.0; y <= 1.0; y++)
                {
                    for(float x = -1.0; x <= 1.0; x++)
                    {
                        float2 offset = float2(x, y);
                        float2 neighbor = hash(id + offset) * 0.6 + 0.2;
                        float2 pt = offset + neighbor - gv;
                        float d = length(pt);
                        minDist = min(minDist, d);
                    }
                }
                
                float star = 1.0 - saturate(minDist);
                star = pow(star, _StarSize);
                
                float twinkle = sin(_Time.y * _StarTwinkleSpeed + hash(id) * 6.28) * 0.5 + 0.5;
                star *= lerp(0.7, 1.0, twinkle);
                
                return star * _StarBrightness;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = normalize(v.vertex.xyz);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.worldPos);
                float h = dir.y * 0.5 + 0.5;
                
                float4 bottomToMid = lerp(_BottomColor, _MidColor, 
                    smoothstep(_MidPoint - _Smoothness, _MidPoint, h));
                float4 midToTop = lerp(_MidColor, _TopColor, 
                    smoothstep(_MidPoint, _MidPoint + _Smoothness, h));
                float4 gradient = lerp(bottomToMid, midToTop, step(_MidPoint, h));
                
                float2 starUV = float2(
                    atan2(dir.x, dir.z) / (2.0 * 3.14159) + 0.5,
                    asin(dir.y) / 3.14159 + 0.5
                );
                
                float starField = stars(starUV);
                
                float horizonFade = pow(saturate(abs(dir.y)), 0.5);
                starField *= horizonFade;
                
                float4 finalColor = gradient + float4(starField, starField, starField, 0);
                
                return finalColor;
            }
            ENDCG
        }
    }
}
