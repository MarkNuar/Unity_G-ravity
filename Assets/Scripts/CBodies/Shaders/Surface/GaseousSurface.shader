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
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

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

        
        // number of octaves of fbm
        #define NUM_NOISE_OCTAVES 10

        // Precision-adjusted variations of https://www.shadertoy.com/view/4djSRW
        float hash(float p) { p = frac(p * 0.011); p *= p + 7.5; p *= p + p; return frac(p); }

        float noise(float3 x) {
            const float3 step = float3(110, 241, 171);
            const float3 i = floor(x);
            const float3 f = frac(x);
            const float n = dot(i, step);
            float3 u = f * f * (3.0 - 2.0 * f);
            return lerp(lerp(lerp( hash(n + dot(step, float3(0, 0, 0))), hash(n + dot(step, float3(1, 0, 0))), u.x),
                           lerp( hash(n + dot(step, float3(0, 1, 0))), hash(n + dot(step, float3(1, 1, 0))), u.x), u.y),
                       lerp(lerp( hash(n + dot(step, float3(0, 0, 1))), hash(n + dot(step, float3(1, 0, 1))), u.x),
                           lerp( hash(n + dot(step, float3(0, 1, 1))), hash(n + dot(step, float3(1, 1, 1))), u.x), u.y), u.z);
        }

        float fbm(float3 x) {
	        float v = 0.0;
	        float a = 0.5;
	        const float3 shift = (0);
	        for (int i = 0; i < NUM_NOISE_OCTAVES; ++i) {
		        v += a * noise(x);
		        x = x * 2.0 + shift;
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
        	//const float v = fbm(x + 5.0*r + _Time.y * 0.005);
        	const float v = fbm(x + 5.0*r + _Time.y * 0.00005);
            
            // convert noise value into color
            // three colors: top - mid - bottom (mid being constructed by three colors)
            // const float3 col_top = float3(1.0, 1.0, 1.0);
            // const float3 col_bot = float3(0.0, 0.0, 0.0);
            // const float3 col_mid1 = float3(0.1, 0.2, 0.0);
            // const float3 col_mid2 = float3(0.7, 0.4, 0.3);
            // const float3 col_mid3 = float3(1.0, 0.4, 0.2);

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