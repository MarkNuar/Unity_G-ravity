// For reference:
// https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html

Shader "Hidden/Ocean"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "../Includes/Math.cginc"
			#include "../Includes/Triplanar.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 viewVector : TEXCOORD1;
			};
			
			v2f vert (appdata v) {
				v2f output;
				output.pos = UnityObjectToClipPos(v.vertex);
				output.uv = v.uv;
				// Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z
				// (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
				float3 view_vector = mul(unity_CameraInvProjection, float4(v.uv * 2 -1, 0, -1));
				output.viewVector = mul(unity_CameraToWorld, float4(view_vector,0));
				return output;
			}

			float4 colA;
			float4 colB;
			float4 specularCol;
			float depthMultiplier;
			float alphaMultiplier;
			float smoothness;
			
			sampler2D waveNormalA;
			sampler2D waveNormalB;
			float waveStrength;
			float waveNormalScale;
			float waveSpeed;

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			float4 params;

			float planetScale;
			float3 oceanCentre;
			float oceanRadius;
			float3 dirToSun;

			float correct_depth(float rawDepth, float viewLength)
			{
			    float persp = LinearEyeDepth(rawDepth) * viewLength;
			    float t = _ProjectionParams.x>0 ? (rawDepth) : (1-rawDepth);
			    float ortho = lerp(_ProjectionParams.y, _ProjectionParams.z, t);
			    return lerp(persp,ortho,unity_OrthoParams.w);
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 original_col = tex2D(_MainTex, i.uv);

				float3 ray_pos;
				float view_length;
				float3 ray_dir;
				if(unity_OrthoParams.w == 0)
				{
					ray_pos = _WorldSpaceCameraPos;
					view_length = length(i.viewVector);
					ray_dir = i.viewVector / view_length;
				}
				else
				{
					const float hw = unity_OrthoParams.x;
					const float hh = unity_OrthoParams.y;
                    float2 uv = i.uv * 2 - 1;
                    ray_pos = float3(_WorldSpaceCameraPos.x + uv.x * hw, _WorldSpaceCameraPos.y + uv.y * hh, _WorldSpaceCameraPos.z);
                    view_length = length(i.viewVector);
                    ray_dir = float3(0, 0, 1);
				}

				const float raw_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				const float scene_depth = correct_depth(raw_depth, view_length);

				//return scene_depth / 1000;
				
				float2 hit_info = ray_sphere(oceanCentre, oceanRadius, ray_pos, ray_dir);
				const float dst_to_ocean = hit_info.x;
				const float dst_through_ocean = hit_info.y;
				const float3 ray_ocean_intersect_pos = ray_pos + ray_dir * dst_to_ocean - oceanCentre;

				// dst that view ray travels through ocean (before hitting terrain / exiting ocean)
				const float ocean_view_depth = min(dst_through_ocean, scene_depth - dst_to_ocean);


				if (ocean_view_depth > 0) {
					//return 1;

					const float3 clip_plane_pos = ray_pos + i.viewVector * _ProjectionParams.y;

					const float dst_above_water = length(clip_plane_pos - oceanCentre) - oceanRadius;

					const float t = 1 - exp(-ocean_view_depth / planetScale * depthMultiplier);
					const float alpha =  1-exp(-ocean_view_depth / planetScale * alphaMultiplier);
					float4 ocean_col = lerp(colA, colB, t);

					const float3 ocean_sphere_normal = normalize(ray_ocean_intersect_pos);

					const float2 wave_offset_a = float2(_Time.x * waveSpeed, _Time.x * waveSpeed * 0.8);
					const float2 wave_offset_b = float2(_Time.x * waveSpeed * - 0.8, _Time.x * waveSpeed * -0.3);
					float3 wave_normal = triplanarNormal(ray_ocean_intersect_pos, ocean_sphere_normal, waveNormalScale / planetScale, wave_offset_a, waveNormalA);
					wave_normal = triplanarNormal(ray_ocean_intersect_pos, wave_normal, waveNormalScale / planetScale, wave_offset_b, waveNormalB);
					wave_normal = normalize(lerp(ocean_sphere_normal, wave_normal, waveStrength));
					//return float4(oceanNormal * .5 + .5,1);
					const float diffuse_lighting = saturate(dot(ocean_sphere_normal, dirToSun));
					const float specular_angle = acos(dot(normalize(dirToSun - ray_dir), wave_normal));
					const float specular_exponent = specular_angle / (1 - smoothness);
					const float specular_highlight = exp(-specular_exponent * specular_exponent);
				
					ocean_col *= diffuse_lighting + 0.05;
					ocean_col += specular_highlight * (dst_above_water > 0) * specularCol;
					
					//return float4(oceanSphereNormal,1);
					float4 final_col =  original_col * (1-alpha) + ocean_col * alpha;
					return float4(final_col.xyz, params.x);
				}
				return original_col;
			}
			ENDCG
		}
	}
}
