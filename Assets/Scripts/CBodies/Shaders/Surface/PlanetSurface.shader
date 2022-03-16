
Shader "CBodies/Planet"
{
	Properties
	{
		[Header(Flat Terrain)]
		_ShoreLow("Shore Low", Color) = (0,0,0,1)
		_ShoreHigh("Shore High", Color) = (0,0,0,1)
		_FlatLowA("Flat Low A", Color) = (0,0,0,1)
		_FlatHighA("Flat High A", Color) = (0,0,0,1)

		_FlatLowB("Flat Low B", Color) = (0,0,0,1)
		_FlatHighB("Flat High B", Color) = (0,0,0,1)

		_FlatColBlend("Colour Blend", Range(0,3)) = 1.5
		_FlatColBlendNoise("Blend Noise", Range(0,1)) = 0.3
		_ShoreHeight("Shore Height", Range(0,0.2)) = 0.05
		_ShoreBlend("Shore Blend", Range(0,0.2)) = 0.03
		_MaxFlatHeight("Max Flat Height", Range(0,1)) = 0.5

		[Header(Steep Terrain)]
		_SteepLow("Steep Colour Low", Color) = (0,0,0,1)
		_SteepHigh("Steep Colour High", Color) = (0,0,0,1)
		_SteepBands("Steep Bands", Range(1, 20)) = 8
		_SteepBandStrength("Band Strength", Range(-1,1)) = 0.5

		[Header(Flat to Steep Transition)]
		_SteepnessThreshold("Steep Threshold", Range(0,1)) = 0.5
		_FlatToSteepBlend("Flat to Steep Blend", Range(0,0.3)) = 0.1
		_FlatToSteepNoise("Flat to Steep Noise", Range(0,0.2)) = 0.1

		[Header(Noise)]
		[NoScaleOffset] _NoiseTex ("Noise Texture", 2D) = "white" {}
		_NoiseZoomXL("Noise Zoom Extra Large", Float) = 1
		_NoiseZoomL("Noise Zoom Large", Float) = 1
		_NoiseZoomM("Noise Zoom Medium", Float) = 1
		_NoiseZoomS("Noise Zoom Small", Float) = 1

		[Header(Other)]
		_FresnelCol("Fresnel Colour", Color) = (1,1,1,1)
		_FresnelStrengthNear("Fresnel Strength Min", float) = 2
		_FresnelStrengthFar("Fresnel Strength Max", float) = 5
		_FresnelPow("Fresnel Power", float) = 2
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		
		[Header(Normals)]
		[NoScaleOffset] _NormalMapFlat ("Normal Map Flat", 2D) = "white" {}
		[NoScaleOffset] _NormalMapSteep ("Normal Map Steep", 2D) = "white" {}
		_NormalMapFlatScale ("Normal Map Flat Scale", Float) = 10
		_NormalMapSteepScale ("Normal Map Steep Scale", Float) = 10
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

		// Flat terrain:
		float4 _ShoreLow;
		float4 _ShoreHigh;

		float4 _FlatLowA;
		float4 _FlatHighA;
		float4 _FlatLowB;
		float4 _FlatHighB;

		float _FlatColBlend;
		float _FlatColBlendNoise;
		float _ShoreHeight;
		float _ShoreBlend;
		float _MaxFlatHeight;

		// Steep terrain
		float4 _SteepLow;
		float4 _SteepHigh;
		float _SteepBands;
		float _SteepBandStrength;

		// Flat to steep transition
		float _SteepnessThreshold;
		float _FlatToSteepBlend;
		float _FlatToSteepNoise;

		// Other:
		float _Glossiness;
		float _Metallic;

		sampler2D _NoiseTex;
		float _NoiseZoomXL;
		float _NoiseZoomL;
		float _NoiseZoomM;
		float _NoiseZoomS;
		
		// Height data:
		float2 heightMinMax;
		float oceanLevel;

		
		sampler2D _NormalMapFlat;
		sampler2D _NormalMapSteep;
		float _NormalMapFlatScale;
		float _NormalMapSteepScale;
		float _NormalMapStrength;

		void surf (Input i, inout SurfaceOutputStandard o)
		{
		
			// Calculate steepness: 0 where totally flat, 1 at max steepness
			float3 sphereNormal = normalize(i.vertPos);
			float steepness = 1 - dot (sphereNormal, i.normal);
			steepness = remap01(steepness, 0, 0.65);
			
			// Calculate heights
			float terrainHeight = length(i.vertPos);
			float shoreHeight = lerp(heightMinMax.x, 1, oceanLevel);
			float aboveShoreHeight01 = remap01(terrainHeight, shoreHeight, heightMinMax.y);
			float flatHeight01 = remap01(aboveShoreHeight01, 0, _MaxFlatHeight);
			
			// Sample noise texture at 4 different scales
			float4 texNoiseZoomXL = triplanar(i.vertPos, i.normal, _NoiseZoomXL, _NoiseTex);
			float4 texNoiseZoomL = triplanar(i.vertPos, i.normal, _NoiseZoomL, _NoiseTex);
			float4 texNoiseZoomM = triplanar(i.vertPos, i.normal, _NoiseZoomM, _NoiseTex);
			float4 texNoiseZoomS = triplanar(i.vertPos, i.normal, _NoiseZoomS, _NoiseTex);
			
			// Flat terrain colour A and B
			float flatColBlendWeight = Blend(0, _FlatColBlend, (flatHeight01-.35) + (texNoiseZoomL.b - 0.5) * _FlatColBlendNoise);
			float3 flatTerrainColA = lerp(_FlatLowA, _FlatHighA, flatColBlendWeight);
			flatTerrainColA = lerp(flatTerrainColA, (_FlatLowA + _FlatHighA)/2 , texNoiseZoomM.g);
			float3 flatTerrainColB = lerp(_FlatLowB, _FlatHighB, flatColBlendWeight);
			flatTerrainColB = lerp(flatTerrainColB, (_FlatLowB + _FlatHighB)/2 , texNoiseZoomM.g);
			
			// Biomes
			float3 flatTerrainCol = lerp(flatTerrainColA, flatTerrainColB, i.vertPos.x);
			
			// Shore
			float shoreBlendWeight = 1-Blend(_ShoreHeight, _ShoreBlend, flatHeight01);
			float4 shoreCol = lerp(_ShoreLow, _ShoreHigh, remap01(aboveShoreHeight01, 0, _ShoreHeight));
			shoreCol = lerp(shoreCol, (_ShoreLow + _ShoreHigh) / 2, texNoiseZoomS.a);
			flatTerrainCol = lerp(flatTerrainCol, shoreCol, shoreBlendWeight);
			
			// Steep terrain colour
			float3 sphereTangent = normalize(float3(-sphereNormal.z, 0, sphereNormal.x));
			float3 normalTangent = normalize(i.normal - sphereNormal * dot(i.normal, sphereNormal));
			float banding = dot(sphereTangent, normalTangent) * .5 + .5;
			banding = (int)(banding * (_SteepBands + 1)) / _SteepBands;
			banding = (abs(banding - 0.5) * 2 - 0.5) * _SteepBandStrength;
			//float3 steepTerrainCol = lerp(_SteepLow, _SteepHigh, aboveShoreHeight01 + banding);
			float3 steepTerrainCol = lerp(_SteepLow, _SteepHigh, aboveShoreHeight01 + banding);
			
			
			// Flat to steep colour transition
			float flatBlendNoise = (texNoiseZoomS.r - 0.5) * _FlatToSteepNoise;
			float flatStrength = 1 - Blend(_SteepnessThreshold + flatBlendNoise, _FlatToSteepBlend, steepness);
			float flatHeightFalloff = 1 - Blend(_MaxFlatHeight + flatBlendNoise, _FlatToSteepBlend, aboveShoreHeight01);
			flatStrength *= flatHeightFalloff;
			
			
			// Set surface colour
			float3 compositeCol = lerp(steepTerrainCol, flatTerrainCol, flatStrength);
			compositeCol = lerp(compositeCol, _FresnelCol, i.fresnel);
			o.Albedo = compositeCol;
			
			// Glossiness
			const float glossiness = dot(o.Albedo, 1) / 3 * _Glossiness;
			o.Smoothness = glossiness;
			o.Metallic = _Metallic;
			
			
			// Sample normal maps:
			// There are two maps, one for steep slopes like mountains and crater walls, and one for flat regions
			// Slopes always use the steep map, but flat regions blend between the flat and steep maps to add variety
			float3 normalMapFlat = triplanar(i.vertPos, i.normal, _NormalMapFlatScale, _NormalMapFlat);
			float3 normalMapSteep = triplanar(i.vertPos, i.normal, _NormalMapSteepScale, _NormalMapSteep);
			
			float3 normal = lerp(normalMapSteep, normalMapFlat, flatStrength);
			// Normal map strength
			normal = lerp(float3(0,0,1), normal, _NormalMapStrength);
			
			o.Normal = normal;
			
		}
		ENDCG
	}
	FallBack "Diffuse"
}
