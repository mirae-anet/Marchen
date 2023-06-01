Shader "Off Axis Studios/2 Color Dissolve"
{
     Properties
    {
        _NoiseScale("Noise Scale", float) = 50
        _MainColor("Main Color", Color) = (0.23, 0.23, 0.23, 1)
        _EdgeColor("Edge Color", Color) = (1, 1, 1, 0.5)
        _EdgeSize("Edge Size", Range(0, -1)) = -0.03333333
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags {"RenderType" = "Transparent" "Queue"="Transparent"}

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _NoiseScale;
            float4 _MainColor;
            float4 _EdgeColor;
            float _EdgeSize;
            float _DissolveAmount;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            inline float NoiseRandom(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
            }

            inline float NoiseInterp(float a, float b, float t)
            {
                return (1.0-t)*a + (t*b);
            }

            inline float ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                uv = abs(frac(uv) - 0.5);
                float2 c0 = i + float2(0.0, 0.0);
                float2 c1 = i + float2(1.0, 0.0);
                float2 c2 = i + float2(0.0, 1.0);
                float2 c3 = i + float2(1.0, 1.0);
                float r0 = NoiseRandom(c0);
                float r1 = NoiseRandom(c1);
                float r2 = NoiseRandom(c2);
                float r3 = NoiseRandom(c3);

                float bottomOfGrid = NoiseInterp(r0, r1, f.x);
                float topOfGrid = NoiseInterp(r2, r3, f.x);
                float t = NoiseInterp(bottomOfGrid, topOfGrid, f.y);

                return t;
            }

            float SimpleNoise(float2 UV, float Scale)
            {
                float t = 0.0;

                float freq = pow(2.0, float(0));
                float amp = pow(0.5, float(3-0));
                t += ValueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

                freq = pow(2.0, float(1));
                amp = pow(0.5, float(3-1));
                t += ValueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

                freq = pow(2.0, float(2));
                amp = pow(0.5, float(3-2));
                t += ValueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

                return t;
            }

            float4 Remap(float4 input, float2 inMinMax, float2 outMinMax)
            {
                return outMinMax.x + (input - inMinMax.x) * (outMinMax.y - outMinMax.x) / (inMinMax.y - inMinMax.x);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float noise = SimpleNoise(i.uv, _NoiseScale);
                float alpha = Remap(noise, float2(-1, 1), float2(0, 1));
                float progress = Remap(_DissolveAmount, float2(-1.25, 1.25), float2(0, 1));
                float offset = alpha + _EdgeSize;
                float offsetStep = step(offset, progress);
                float4 edge = offsetStep * _EdgeColor;
                float inner = abs(float4(1,1,1,1) - offsetStep);
                float4 inside = inner * _MainColor;

                clip(alpha-progress);

                return inside + edge;
            }
            ENDCG
        }
    }
}