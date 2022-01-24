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

			// todo : add shadows to the ring
			float3 light_direction;
			
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 originalCol = tex2D(_MainTex, i.uv);

				if(!has_ring)
					return originalCol;
				
				float3 rayPos;
				float viewLength;
				float3 rayDir;
				if(unity_OrthoParams.w == 0)
				{
					rayPos = _WorldSpaceCameraPos;
					viewLength = length(i.viewVector);
					rayDir = i.viewVector / viewLength;
				}
				else
				{
					const float hw = unity_OrthoParams.x;
					const float hh = unity_OrthoParams.y;
                    float2 uv = i.uv * 2 - 1;
                    rayPos = float3(_WorldSpaceCameraPos.x + uv.x * hw, _WorldSpaceCameraPos.y + uv.y * hh, _WorldSpaceCameraPos.z);
                    viewLength = length(i.viewVector);
                    rayDir = float3(0, 0, 1);
				}

				// float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				// float sceneDepth = correctDepth(nonlin_depth, viewLength);

				

				const float inner_radius = inner_radius_percent * c_body_radius;
				const float outer_radius = outer_radius_percent * c_body_radius;
				const float2 d_ring = intersect_ring( ring_normal, c_body_center, inner_radius, outer_radius, rayPos, rayDir);
				const float d_c_body = (raySphere(c_body_center, c_body_radius, rayPos, rayDir)).x;
				if(d_ring.x < maxFloat && d_ring.x < d_c_body)
				{
					return d_ring.y;
				}
				
				return originalCol;
				
			}
			ENDCG
		}
	}
}
