﻿// Shadertoy source:
// https://www.shadertoy.com/view/tltXWM

// Noise functions and most of the implementation based on
// https://www.shadertoy.com/view/4dS3Wd by Morgan McGuire @morgan3d!

// see also
// http://www.iquilezles.org/www/articles/warp/warp.htm
// https://thebookofshaders.com/13/
// for information on fbm, noise, ...

// please check out stuff like: https://www.shadertoy.com/view/lsGGDd
// for more advanced planet lighting/clouds/...

Shader "CBodies/Gaseous"
{
    Properties {
    	_Glossiness ("Smoothness", Range(0,1)) = 0.0
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        // #pragma surface surf Standard vertex:vert fullforwardshadows
        #pragma surface surf Standard vertex:vert 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        struct Input {
            float2 uv_MainTex;
            float4 objPos;
        };

        void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
        	o.objPos = v.vertex;
		}
        

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;


        float random_val;

        float3 col_top;
        float3 col_bot;
        float3 col_mid1;
        float3 col_mid2;
        float3 col_mid3;

        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assume uniform scaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
		// hash based 3d value noise
		// function taken from https://www.shadertoy.com/view/XslGRr
		// Created by inigo quilez - iq/2013
		// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
		// ported from GLSL to HLSL
        float hash( float n )
		{
		    return frac(sin(n)*43758.5453);
		}
		float noise( float3 x )
		{
		    // The noise function returns a value in the range -1.0f -> 1.0f
		    float3 p = floor(x);
		    float3 f = frac(x);
		    f = f*f*(3.0-2.0*f);
		    const float n = p.x + p.y*57.0 + 113.0*p.z;
		    return lerp(lerp(lerp( hash(n+0.0), hash(n+1.0),f.x),
		                   lerp( hash(n+57.0), hash(n+58.0),f.x),f.y),
		               lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
		                   lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
		}
        
        
        // number of octaves of fbm
        // #define NUM_NOISE_OCTAVES 10
        #define NUM_NOISE_OCTAVES 5

        float fbm(float3 x) {
	        float v = 0.0;
	        float a = 0.5;
	         for (int i = 0; i < NUM_NOISE_OCTAVES; ++i) {
		         v += a * noise(x);
		         x = x * 2.0;
		         a *= 0.5;
	         }
	        return v;
        }

        // returns max of a single float3
        float max3 (float3 v) {
          return max (max (v.x, v.y), v.z);
        }
        
         void surf (Input IN, inout SurfaceOutputStandard o) {
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;

            const float3 x = IN.objPos + random_val * 10;
        	
            // calculate fbm noise (3 steps)
            const float3 q = float3(fbm(x + 0.025 * _Time.y), fbm(x), fbm(x));
            const float3 r = float3(fbm(x + 1.0*q + 0.01 * _Time.y), fbm(x + q), fbm(x + q));
        	const float v = fbm(x + 5.0*r + _Time.y * 0.00005);
            
            // lerp mid color based on intermediate results
            float3 col_mid = lerp(col_mid1, col_mid2, clamp(r, 0.0, 1.0));
            col_mid = lerp(col_mid, col_mid3, clamp(q, 0.0, 1.0));

            // calculate pos (scaling between top and bot color) from v
            const float pos = v * 2.0 - 1.0;
            float3 color = lerp(col_mid, col_top, clamp(pos, 0.0, 1.0));
            color = lerp(color, col_bot, clamp(-pos, 0.0, 1.0));

            // clamp color to scale the highest r/g/b to 1.0
            color = color / max3(color);
              
            // create output color, increase light > 0.5 (and add a bit to dark areas)
            color = (clamp((0.4 * pow(v,3.) + pow(v,2.) + 0.5*v), 0.0, 1.0) * 0.9 + 0.1) * color;
        	
            o.Albedo = color;
        }
       
        ENDCG
    }
    FallBack "Diffuse"
}