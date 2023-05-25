Shader "Off Axis Studios/Gradient Simple"
{
    Properties
    {
      _MainTex("Main Tex", 2D) = "white" {}
      _HeightMin("Gradient Height Min", Float) = -0.5
      _HeightMax("Gradient Height Max", Float) = 0.4
      _ColorMin("Color at Min", Color) = (1,0.78,0,1)
      _ColorMax("Color at Max", Color) = (1,0.1,0,1)
      _Glossiness("Smoothness", Range(0,1)) = 0
      _Metallic("Metallic", Range(0,1)) = 0
      _MetallicMap("Metallic Map", 2D) = "white" {}
    }

        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows
            #pragma target 3.0
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            sampler2D _MainTex;
            fixed4 _ColorMin;
            fixed4 _ColorMax;
            float _HeightMin;
            float _HeightMax;
            half _Glossiness;
            half _Metallic;

            struct Input{
                float2 uv_MainTex;
                float3 worldPos;
            };

            void surf(Input IN, inout SurfaceOutputStandard o){
                half4 c = tex2D(_MainTex, IN.uv_MainTex);
                float h = (_HeightMax - IN.worldPos.y) / (_HeightMax - _HeightMin);
                fixed4 tintColor = lerp(_ColorMax.rgba, _ColorMin.rgba, h);
                o.Albedo = c.rgb * tintColor.rgb;
                o.Alpha = c.a * tintColor.a;

                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
            }
            ENDCG
        }
        Fallback "Diffuse"
}