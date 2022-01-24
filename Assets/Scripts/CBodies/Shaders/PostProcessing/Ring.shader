// For reference:
// https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html

Shader "Hidden/Ring"
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
				float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 -1, 0, -1));
				output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
				return output;
			}
			
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;

			float3 c_body_center;
			float c_body_radius;

			bool has_ring;
			float3 ring_normal;
			float inner_radius_percent;
			float outer_radius_percent;
			float seed;

			float3 ring_color;

			// todo : add shadows to the ring
			float3 light_direction;


			// Functions for ring coloring taken from this shader:
			// https://www.shadertoy.com/view/3scXR7


			float hash1(float p) {
			    p = frac(p * .1031);
			    p *= p + 33.33;
			    p *= p + p;
			    return frac(p);
			}
			
			float noise1(float p) {
			    float i = floor(p);
			    float f = frac(p);
			    float u = f * f * (3.0 - 2.0 * f);
			    return 1.0 - 2.0 * lerp(hash1(i), hash1(i + 1.0), u);
			}
			
			float fbm1(float p) {
			    float f = noise1(p); p = 2.0 * p;
			    f += 0.5 * noise1(p); p = 2.0 * p;
			    f += 0.25 * noise1(p); p = 2.0 * p;
			    f += 0.125 * noise1(p); p = 2.0 * p;
			    f += 0.0625 * noise1(p);
			    return f / 1.9375;
			}

			float correct_depth(float rawDepth, float viewLength)
			{
			    float persp = LinearEyeDepth(rawDepth) * viewLength;
			    float t = _ProjectionParams.x>0 ? (rawDepth) : (1-rawDepth);
			    float ortho = lerp(_ProjectionParams.y, _ProjectionParams.z, t);
			    return lerp(persp,ortho,unity_OrthoParams.w);
			}

			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 originalCol = tex2D(_MainTex, i.uv);

				if(!has_ring)
					return originalCol;
				
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

				const float inner_radius = inner_radius_percent * c_body_radius;
				const float outer_radius = outer_radius_percent * c_body_radius;
				
				const float2 hit_info = intersect_ring( ring_normal, c_body_center, inner_radius, outer_radius, ray_pos, ray_dir);
				
				if(scene_depth - hit_info.x > 0)
				{
					ring_color = pow( ring_color, ( 0.20 ) );
					const float alpha = smoothstep(-0.2, 1.0, fbm1(0.2 * (hit_info.y + 3.0 * fbm1(sin(_Time.y * 0.005f)))));
					//const float alpha = smoothstep(-0.2, 1.0, fbm1(0.5 * (hit_info.y + 3.0 * seed)));

					const float3 vertex_pos = ray_pos + hit_info.x * ray_dir;
					float sphere_hit_info = ray_sphere(c_body_center, c_body_radius, vertex_pos, light_direction);
					if(sphere_hit_info.x < maxFloat)
						ring_color *= 0.1;
					
					
					
					return float4(lerp(originalCol, ring_color, alpha), 1);
				}
				
				return originalCol;
				
			}
			ENDCG
		}
	}
}
