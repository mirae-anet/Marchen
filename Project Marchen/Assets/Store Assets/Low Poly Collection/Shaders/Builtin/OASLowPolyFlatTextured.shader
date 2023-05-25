﻿Shader "Off Axis Studios/Low Poly Style Flat (Textured)"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_Shininess ("Shininess", Float) = 10
		_LightIntensityMultiplier ("Light Intensity Multiplier", Float) = 1
	}
		SubShader
	{

		Tags{ "RenderType" = "Opaque" }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Tags { "LightMode" = "ForwardBase"}

			CGPROGRAM

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			uniform float4 _Color;
			uniform sampler2D _MainTex;
			uniform float _Shininess;
			uniform float _LightIntensityMultiplier;

			struct v2g
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL;
				float2 uv : TEXCOORD0;
				float3 vertex : TEXCOORD1;
				float3 vertexLighting : TEXCOORD2;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL;
				float2 uv : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 vertexLighting : TEXCOORD2;
				LIGHTING_COORDS(3, 4)
			};

			v2g vert(appdata_full v)
			{
				v2g OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.norm = v.normal;
				OUT.uv = v.texcoord;
				OUT.vertex = v.vertex;

				float3 vertexLighting = float3(0, 0, 0);
				#ifdef VERTEXLIGHT_ON
				for (int index = 0; index < 4; index++)
				{
					float3 normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
					float3 lightPosition = float3(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index]);
					float3 vertexToLightSource = lightPosition - mul(unity_ObjectToWorld, v.vertex);
					float3 lightDir = normalize(vertexToLightSource);
					float distanceSquared = dot(vertexToLightSource, vertexToLightSource);
					float attenuation = 1.0 / (1.0 + unity_4LightAtten0[index] * distanceSquared);

					vertexLighting += attenuation * unity_LightColor[index].rgb * _Color.rgb * saturate(dot(normalDir, lightDir));
				}
				#endif
				OUT.vertexLighting = vertexLighting;

				return OUT;
			}

			[maxvertexcount(3)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
			{
				float3 v0 = IN[0].pos.xyz;
				float3 v1 = IN[1].pos.xyz;
				float3 v2 = IN[2].pos.xyz;

				g2f OUT;
				OUT.norm = normalize(IN[0].norm + IN[1].norm + IN[2].norm);
				OUT.uv = (IN[0].uv + IN[1].uv + IN[2].uv) / 3;
				OUT.vertexLighting = (IN[0].vertexLighting + IN[1].vertexLighting + IN[2].vertexLighting) / 3;
				OUT.posWorld = mul(unity_ObjectToWorld, (IN[0].vertex + IN[1].vertex + IN[2].vertex) / 3);

				OUT.pos = IN[0].pos;
				TRANSFER_VERTEX_TO_FRAGMENT(OUT);
				triStream.Append(OUT);

				OUT.pos = IN[1].pos;
				TRANSFER_VERTEX_TO_FRAGMENT(OUT);
				triStream.Append(OUT);

				OUT.pos = IN[2].pos;
				TRANSFER_VERTEX_TO_FRAGMENT(OUT);
				triStream.Append(OUT);
			}

			half4 frag(g2f IN) : COLOR
			{
				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - IN.posWorld.xyz);
				float3 normalDir = normalize(mul(float4(IN.norm, 0.0), unity_WorldToObject).xyz);
				float3 vertexToLight = _WorldSpaceLightPos0.w == 0 ? _WorldSpaceLightPos0.xyz : _WorldSpaceLightPos0.xyz - IN.posWorld.xyz;
				float3 lightDir = normalize(vertexToLight);

				float3 ambientLight = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb;
				UNITY_LIGHT_ATTENUATION(atten, IN, IN.posWorld);
				float3 diffuseReflection = atten * _LightColor0.rgb * _Color.rgb * saturate(dot(normalDir, lightDir));

				float3 specularReflection = float3(0.0, 0.0, 0.0);
				if (dot(normalDir, lightDir) >= 0.0) 
				{
				   specularReflection = atten * _LightColor0.rgb * pow(max(0.0, dot(reflect(-lightDir, normalDir), viewDir)), _Shininess);
				}

				float4 colorTex = tex2D(_MainTex, IN.uv);
				return float4((IN.vertexLighting + (ambientLight * _LightIntensityMultiplier) + diffuseReflection + specularReflection) * colorTex, _Color.a);
			}

				ENDCG
			}
		
			Pass
			{
				Tags{ "LightMode" = "ForwardAdd" }
				Blend One One
				ZWrite Off

				CGPROGRAM

				#pragma vertex vert
				#pragma geometry geom
				#pragma fragment frag
				#pragma multi_compile_fwdadd_fullshadows

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "Lighting.cginc"

				uniform float4 _Color;
				uniform sampler2D _MainTex;
				uniform float _Shininess;
				uniform float _LightIntensityMultiplier;

				struct v2g
				{
					float3 norm : NORMAL;
					float3 vertex : TEXCOORD0;
					float3 uv : TEXCOORD1;
				};

				struct g2f
				{
					float4 pos : SV_POSITION;
					float3 norm : NORMAL;
					float4 posWorld : TEXCOORD0;
					float3 uv : TEXCOORD1;
					LIGHTING_COORDS(3, 4)
				};

				struct unityTransferVertexToFragmentSucksHack
				{
					float4 vertex : SV_POSITION;
				};

				appdata_full vert(appdata_full v)
				{
					appdata_full OUT;
					OUT = v;
					return OUT;
				}

				[maxvertexcount(3)]
				void geom(triangle appdata_full IN[3], inout TriangleStream<g2f> triStream)
				{
					g2f OUT;
					OUT.norm = normalize((IN[0].normal + IN[1].normal + IN[2].normal) / 3);
					OUT.uv = (IN[0].texcoord + IN[1].texcoord + IN[2].texcoord) / 3;

					unityTransferVertexToFragmentSucksHack v;

					v.vertex = IN[0].vertex;
					OUT.pos = UnityObjectToClipPos(v.vertex);
					OUT.posWorld = mul(unity_ObjectToWorld, v.vertex);
					TRANSFER_VERTEX_TO_FRAGMENT(OUT);
					triStream.Append(OUT);

					v.vertex = IN[1].vertex;
					OUT.pos = UnityObjectToClipPos(v.vertex);
					OUT.posWorld = mul(unity_ObjectToWorld, v.vertex);
					TRANSFER_VERTEX_TO_FRAGMENT(OUT);
					triStream.Append(OUT);

					v.vertex = IN[2].vertex;
					OUT.pos = UnityObjectToClipPos(v.vertex);
					OUT.posWorld = mul(unity_ObjectToWorld, v.vertex);
					TRANSFER_VERTEX_TO_FRAGMENT(OUT);
					triStream.Append(OUT);
				}

				float4 frag(g2f IN) : COLOR
				{
					float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - IN.posWorld.xyz);
					float3 normalDir = normalize(mul(float4(IN.norm, 0.0), unity_WorldToObject).xyz);
					float3 vertexToLight = _WorldSpaceLightPos0.w == 0 ? _WorldSpaceLightPos0.xyz : _WorldSpaceLightPos0.xyz - IN.posWorld.xyz;
					float3 lightDir = normalize(vertexToLight);

					UNITY_LIGHT_ATTENUATION(atten, IN, IN.posWorld.xyz);

					float3 specularReflection = float3(0.0, 0.0, 0.0);
					if (dot(normalDir, lightDir) >= 0.0) 
					{
					   specularReflection = atten * _LightColor0.rgb * pow(max(0.0, dot(reflect(-lightDir, normalDir), viewDir)), _Shininess);
					}
					
					float3 diffuseReflection = atten * _LightColor0.rgb * _Color.rgb * _LightIntensityMultiplier * max(0.0, dot(normalDir, lightDir));
					float4 colorTex = tex2D(_MainTex, IN.uv);
					return float4((diffuseReflection + specularReflection) * colorTex, _Color.a);
				}

				ENDCG
			}
	}
	Fallback "Standard"
}