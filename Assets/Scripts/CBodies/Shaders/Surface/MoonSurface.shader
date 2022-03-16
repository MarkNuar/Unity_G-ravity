
Shader "CBodies/Moon"
{
	Properties
	{
		[Header(Noise)]
		[NoScaleOffset] _CraterNoiseTex ("Crater Noise Texture", 2D) = "white" {}
		_CraterNoiseZoom("Noise Zoom", Float) = 1

		[Header(Other)]
		_FresnelCol("Fresnel Colour", Color) = (1,1,1,1)
		_FresnelStrengthNear("Fresnel Strength Min", float) = 2
		_FresnelStrengthFar("Fresnel Strength Max", float) = 5
		_FresnelPow("Fresnel Power", float) = 2
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		
		[Header(Normals)]
		[NoScaleOffset] _NormalMapCraters ("Normal Map Craters", 2D) = "white" {}
		_NormalMapCratersScale ("Normal Map Craters Scale", Float) = 10
		_NormalMapStrength ("Normal Map Strength", Range(0,1)) = 0.3
		
		}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert 
		#pragma target 3.5

		#include "../Includes/Triplanar.cginc"
		#include "../Includes/Math.cginc"
		
		float4 _FresnelCol;
		float _FresnelStrengthNear;
		float _FresnelStrengthFar;
		float _FresnelPow;
		float bodyScale;

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
			float4 terrainData;
			float3 vertPos;
			float3 normal;
			float4 tangent;
			float fresnel;
		};

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertPos = v.vertex;
			o.normal = v.normal;
			o.terrainData = v.texcoord;
			o.tangent = v.tangent;

			// Fresnel (fade out when close to body)
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float3 bodyWorldCentre = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
			float camRadiiFromSurface = (length(bodyWorldCentre - _WorldSpaceCameraPos.xyz) - bodyScale) / bodyScale;
			float fresnelT = smoothstep(0,1,camRadiiFromSurface);
			float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
			float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal,0)));
			float fresStrength = lerp(_FresnelStrengthNear, _FresnelStrengthFar, fresnelT);
			o.fresnel = saturate(fresStrength * pow(1 + dot(viewDir, normWorld), _FresnelPow));
		}

		
		float4 primaryColorA;
		float4 secondaryColorA;
		float4 primaryColorB;
		float4 secondaryColorB;

		// Other:
		float _Glossiness;
		float _Metallic;
		
		sampler2D _CraterNoiseTex;
		float _CraterNoiseZoom;

		// Biome Properties
		float _BiomeBlendStrength;
		float _BiomeWarpStrength;
		float4 _RandomBiomeValues;
		
		// Height data:
		float2 heightMinMax;
		float oceanLevel;

		// Normal maps
		sampler2D _NormalMapCraters;
		float _NormalMapCratersScale;
		float _NormalMapStrength;


		void surf (Input i, inout SurfaceOutputStandard o)
		{			
			float3 sphereNormal = normalize(i.vertPos);
			float steepness = 1 - dot (sphereNormal, i.normal);

			
			float4 texCraterNoise = triplanar(i.vertPos, i.normal, _CraterNoiseZoom, _CraterNoiseTex);
			
			
			// No Atmosphere color
			// Use FlatLowA, FlatHighA, FlatLowB, FlatHighB as colors
			float height01 = remap01(length(i.vertPos), heightMinMax.x, heightMinMax.y);
			float warpNoise = Blend(0, 3, i.terrainData.z);

			// Blend between primary and secondary colours by height (plus some noise)
			float heightNoiseA = -texCraterNoise.g * steepness - (texCraterNoise.b - 0.5) * 0.7 + (texCraterNoise.a - 0.5) * _RandomBiomeValues.x;
			float heightNoiseB = (texCraterNoise.g-0.5) * _RandomBiomeValues.y + (texCraterNoise.r-0.5) * _RandomBiomeValues.z;
			float heightBlendWeightA = Blend(0.5, 0.6, height01 + heightNoiseA) * warpNoise;
			float heightBlendWeightB = Blend(0.5, 0.6, height01 + heightNoiseB) * warpNoise;
			float3 colBiomeA = lerp(primaryColorA, secondaryColorA, heightBlendWeightA);
			float3 colBiomeB = lerp(primaryColorB, secondaryColorB, heightBlendWeightB);
			// Blend between colour A and B based on terrain data noise
			float biomeNoise = dot(texCraterNoise.ga - 0.5, _RandomBiomeValues.zw) * 4;
			float biomeWeight = Blend(5 * 0.8 + i.terrainData.z * _BiomeWarpStrength, _BiomeBlendStrength + warpNoise * 15,  biomeNoise);
			float3 craterCol = lerp(colBiomeA, colBiomeB, biomeWeight);
			
			o.Albedo = craterCol;
			
			// Normal
			float3 normal = triplanar(i.vertPos, i.normal, _NormalMapCratersScale, _NormalMapCraters);
			normal = lerp(float3(0,0,1), normal, _NormalMapStrength);
			o.Normal = normal;
			
			// Glossiness
			const float glossiness = dot(o.Albedo, 1) / 3 * _Glossiness;
			o.Smoothness = glossiness;
			o.Metallic = _Metallic;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
