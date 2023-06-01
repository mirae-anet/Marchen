Shader "Off Axis Studios/Triplanar Normal Light Ramp" {

	Properties{
		_TopColor("Top Color", Color) = (0.27,0.51,0.34,1)
		_TopTex("Top Texture", 2D) = "white" {}
		_TopNormal("Top Normal", 2D) = "bump" {}
		_TopScale("Top Scale", Range(-2,2)) = 1
		_TopSpread("Top Spread", Range(-5,5)) = 2
		_EdgeWidth("Edge Width", Range(0,0.5)) = 0.5
		_BaseColor("Base Color", Color) = (0.5,0.5,0.5,1)
		_BaseTex("Base Texture", 2D) = "white" {}
		_BaseNormal("Base Normal", 2D) = "bump" {}
		_BaseScale("Side Scale", Range(-2,2)) = 1
		_LightRamp("Light Ramp", 2D) = "gray" {}
		_Noise("Noise", 2D) = "white" {}
		_NoiseScale("Noise Scale", Range(-2,2)) = 1
		_RimPower("Rim Light Power", Range(-2,20)) = 20
		_RimTopColor("Rim Light Color Top", Color) = (0.5,0.5,0.5,1)
		_RimBaseColor("Rim Light Base Color", Color) = (0.5,0.5,0.5,1)
	}

		SubShader{
			Tags{ "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma surface surf ToonRamp
			#pragma lighting ToonRamp exclude_path:prepass

			sampler2D _LightRamp;
			sampler2D _TopTex;
			sampler2D _BaseTex;
			sampler2D _BaseNormal;
			sampler2D _Noise;
			sampler2D _TopNormal;
			float4 _Color;
			float4 _RimTopColor;
			float4 _RimBaseColor;
			float4 _TopColor;
			float4 _BaseColor;
			float _RimPower;
			float  _TopSpread;
			float _EdgeWidth;
			float _TopScale;
			float _BaseScale;
			float _NoiseScale;

			struct Input {
				float2 uv_MainTex : TEXCOORD0;
				float3 worldPos;
				float3 worldNormal; INTERNAL_DATA
				float3 viewDir;
			};

			inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten){
				#ifndef USING_DIRECTIONAL_LIGHT
				lightDir = normalize(lightDir);
				#endif

				half d = dot(s.Normal, lightDir) * 0.5 + 0.5;
				half3 ramp = tex2D(_LightRamp, float2(d,d)).rgb;
				half4 c;

				c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
				c.a = 0;

				return c;
			}

			void surf(Input IN, inout SurfaceOutput o) {
				float3 worldNormalE = WorldNormalVector(IN, o.Normal);
				float3 blendNormal = saturate(pow(worldNormalE * 1.4,4));

				float3 xn = tex2D(_Noise, IN.worldPos.zy * _NoiseScale);
				float3 yn = tex2D(_Noise, IN.worldPos.zx * _NoiseScale);
				float3 zn = tex2D(_Noise, IN.worldPos.xy * _NoiseScale);

				float3 noisetexture = zn;
				noisetexture = lerp(noisetexture, xn, blendNormal.x);
				noisetexture = lerp(noisetexture, yn, blendNormal.y);

				float3 xm = tex2D(_TopTex, IN.worldPos.zy * _TopScale);
				float3 zm = tex2D(_TopTex, IN.worldPos.xy * _TopScale);
				float3 ym = tex2D(_TopTex, IN.worldPos.zx * _TopScale);

				float3 toptexture = zm;
				toptexture = lerp(toptexture, xm, blendNormal.x);
				toptexture = lerp(toptexture, ym, blendNormal.y);

				float3 xnnt = UnpackNormal(tex2D(_TopNormal, IN.worldPos.zy * _TopScale));
				float3 znnt = UnpackNormal(tex2D(_TopNormal, IN.worldPos.xy * _TopScale));
				float3 ynnt = UnpackNormal(tex2D(_TopNormal, IN.worldPos.zx * _TopScale));

				float3 toptextureNormal = znnt;
				toptextureNormal = lerp(toptextureNormal, xnnt, blendNormal.x);
				toptextureNormal = lerp(toptextureNormal, ynnt, blendNormal.y);

				float3 xnn = UnpackNormal(tex2D(_BaseNormal, IN.worldPos.zy * _BaseScale));
				float3 znn = UnpackNormal(tex2D(_BaseNormal, IN.worldPos.xy * _BaseScale));
				float3 ynn = UnpackNormal(tex2D(_BaseNormal, IN.worldPos.zx * _BaseScale));

				float3 sidetextureNormal = znn;
				sidetextureNormal = lerp(sidetextureNormal, xnn, blendNormal.x);
				sidetextureNormal = lerp(sidetextureNormal, ynn, blendNormal.y);

				float3 x = tex2D(_BaseTex, IN.worldPos.zy * _BaseScale);
				float3 y = tex2D(_BaseTex, IN.worldPos.zx * _BaseScale);
				float3 z = tex2D(_BaseTex, IN.worldPos.xy * _BaseScale);

				float3 sidetexture = z;
				sidetexture = lerp(sidetexture, x, blendNormal.x);
				sidetexture = lerp(sidetexture, y, blendNormal.y);

				float worldNormalDotNoise = dot(o.Normal + (noisetexture.y + (noisetexture * 0.5)), worldNormalE.y);

				float3 topTextureResult = step(_TopSpread, worldNormalDotNoise) * toptexture * _TopColor;
				float3 topNormalResult = step(_TopSpread, worldNormalDotNoise) * toptextureNormal;

				float3 sideTextureResult = step(worldNormalDotNoise, _TopSpread) * sidetexture * _BaseColor;
				float3 sideNormalResult = step(worldNormalDotNoise, _TopSpread) * sidetextureNormal;

				float3 topTextureEdgeResult = step(_TopSpread, worldNormalDotNoise) * step(worldNormalDotNoise, _TopSpread + _EdgeWidth) * -0.15;

				o.Normal = topNormalResult + sideNormalResult;

				o.Albedo = topTextureResult + sideTextureResult + topTextureEdgeResult;

				half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal * noisetexture));

				half rim2 = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
				o.Emission = step(_TopSpread, worldNormalDotNoise) * _RimTopColor.rgb * pow(rim, _RimPower) + step(worldNormalDotNoise, _TopSpread) * _RimBaseColor.rgb * pow(rim2, _RimPower);
			}
		ENDCG
		}
		Fallback "Diffuse"
}