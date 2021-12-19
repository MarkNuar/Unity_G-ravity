

#include <UnityShaderVariables.cginc>

#include "Assets/Scripts/CBodies/Shaders/Includes/Math.cginc"

void Compute_Surface_Color_float(out float3 albedo)
{
    // Flat terrain colour A and B
	float flatColBlendWeight = Blend(0, _FlatColBlend, (flatHeight01-.5) + (texNoise.b - 0.5) * _FlatColBlendNoise);
	float3 flatTerrainColA = lerp(_FlatLowA, _FlatHighA, flatColBlendWeight);
	flatTerrainColA = lerp(flatTerrainColA, (_FlatLowA + _FlatHighA) / 2, texNoise.a);
	float3 flatTerrainColB = lerp(_FlatLowB, _FlatHighB, flatColBlendWeight);
	flatTerrainColB = lerp(flatTerrainColB, (_FlatLowB + _FlatHighB) / 2, texNoise.a);

	// Biome
	float biomeWeight = Blend(_TestParams.x, _TestParams.y,IN.terrainData.x);
	biomeWeight = Blend(0, _TestParams.z, IN.vertPos.x + IN.terrainData.x * _TestParams.x + IN.terrainData.y * _TestParams.y);
	float3 flatTerrainCol = lerp(flatTerrainColA, flatTerrainColB, biomeWeight);

	// Shore
	float shoreBlendWeight = 1-Blend(_ShoreHeight, _ShoreBlend, flatHeight01);
	float4 shoreCol = lerp(_ShoreLow, _ShoreHigh, remap01(aboveShoreHeight01, 0, _ShoreHeight));
	shoreCol = lerp(shoreCol, (_ShoreLow + _ShoreHigh) / 2, texNoise.g);
	flatTerrainCol = lerp(flatTerrainCol, shoreCol, shoreBlendWeight);

	// Steep terrain colour
	float3 sphereTangent = normalize(float3(-sphereNormal.z, 0, sphereNormal.x));
	float3 normalTangent = normalize(IN.normal - sphereNormal * dot(IN.normal, sphereNormal));
	float banding = dot(sphereTangent, normalTangent) * .5 + .5;
	banding = (int)(banding * (_SteepBands + 1)) / _SteepBands;
	banding = (abs(banding - 0.5) * 2 - 0.5) * _SteepBandStrength;
	float3 steepTerrainCol = lerp(_SteepLow, _SteepHigh, aboveShoreHeight01 + banding);
	
	// Flat to steep colour transition
	float flatBlendNoise = (texNoise2.r - 0.5) * _FlatToSteepNoise;
	float flatStrength = 1 - Blend(_SteepnessThreshold + flatBlendNoise, _FlatToSteepBlend, steepness);
	float flatHeightFalloff = 1 - Blend(_MaxFlatHeight + flatBlendNoise, _FlatToSteepBlend, aboveShoreHeight01);
	flatStrength *= flatHeightFalloff;
	
	// Snowy poles
	float3 snowCol = 0;
	float snowWeight = 0;
	float snowLineNoise = IN.terrainData.y * _SnowNoiseA * 0.01 + (texNoise.b-0.5) * _SnowNoiseB * 0.01;
	snowWeight = Blend(_SnowLongitude, _SnowBlend, abs(IN.vertPos.y + snowLineNoise)) * _UseSnowyPoles;
	float snowSpeckle = 1 - texNoise2.g * 0.5 * 0.1;
	snowCol = _SnowCol * lerp (1, _SnowHighlight, aboveShoreHeight01 + banding) * snowSpeckle;

	// Fresnel (fade out when close to body)
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	float3 bodyWorldCentre = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
	float camRadiiFromSurface = (length(bodyWorldCentre - _WorldSpaceCameraPos.xyz) - bodyScale) / bodyScale;
	float fresnelT = smoothstep(0,1,camRadiiFromSurface);
	float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
	float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal,0)));
	float fresStrength = lerp(_FresnelStrengthNear, _FresnelStrengthFar, fresnelT);
	fresnel = saturate(fresStrength * pow(1 + dot(viewDir, normWorld), _FresnelPow));
	
	// Set surface colour
	float3 compositeCol = lerp(steepTerrainCol, flatTerrainCol, flatStrength);
	compositeCol = lerp(compositeCol, snowCol, snowWeight);
	compositeCol = lerp(compositeCol, _FresnelCol, fresnel);
}
