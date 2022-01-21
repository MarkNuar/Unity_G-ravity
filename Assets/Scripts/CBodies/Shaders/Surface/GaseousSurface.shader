// This is a fork of Procgen Planet from https://www.shadertoy.com/view/tltXWM .
// The problem of Procgen Planet is that there are visible noise artifacts
// (white dots randomly appearing).
//
// This fork uses the reference Perlin 3D function from https://mrl.nyu.edu/~perlin/noise/
// which fixes the white flashes.
//
// Noise functions and most of the implementation based on
// https://www.shadertoy.com/view/4dS3Wd by Morgan McGuire @morgan3d!

// see also
// http://www.iquilezles.org/www/articles/warp/warp.htm
// https://thebookofshaders.com/13/
// for informations on fbm, noise, ...

// please check out stuff like: https://www.shadertoy.com/view/lsGGDd
// for more advanced planet lighting/clouds/...

Shader "CBodies/Gaseous"
{
    Properties
    {
        _Glossiness ("Smoothness", Range(0,1)) = 0.0
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert
        
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        #include "../Includes/Math.cginc"

        half _Glossiness;
        half _Metallic;

        int randomSeed;

        float3 col_top;
        float3 col_bot;
        float3 col_mid1;
        float3 col_mid2;
        float3 col_mid3;
        

        struct Input
        {
            float2 uv_MainTex;
            float3 objPos;
        };
        
        void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
        	o.objPos = v.vertex;
		}
        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        #define mod(x,y) (x-y*floor(x/y))

        static int perm[] = {
            151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
            151
        };
        
        int perm1plus(int i, int k) { return int(mod(float(perm[i]) + float(k), 256.)); }

        float Fade(float t) { return t * t * t * (t * (t * 6. - 15.) + 10.); }

        float Grad(int hash, float x, float y, float z) {
            int h = hash & 15;
            float u = h < 8 ? x : y;
            float v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        float Noise3D(float3 val) {
            float3 v1 = floor(val);
            float3 v2 = frac(val);
            int X = int(mod(v1.x, 256.));
            int Y = int(mod(v1.y, 256.));
            int Z = int(mod(v1.z, 256.));
            float x = v2.x;
            float y = v2.y;
            float z = v2.z;
            float u = Fade(x);
            float v = Fade(y);
            float w = Fade(z);
            int A  = perm1plus(X, Y);
            int B  = perm1plus(X+1, Y);
            int AA = perm1plus(A, Z);
            int BA = perm1plus(B, Z);
            int AB = perm1plus(A+1, Z);
            int BB = perm1plus(B+1, Z);

            return lerp(lerp(lerp(Grad(perm[AA  ], x, y   , z  ),  Grad(perm[BA  ], x-1., y   , z  ), u),
                           lerp(Grad(perm[AB  ], x, y-1., z  ),  Grad(perm[BB  ], x-1., y-1., z  ), u),
                           v),
                       lerp(lerp(Grad(perm[AA+1], x, y   , z-1.), Grad(perm[BA+1], x-1., y   , z-1.), u),
                           lerp(Grad(perm[AB+1], x, y-1., z-1.), Grad(perm[BB+1], x-1., y-1., z-1.), u),
                           v),
                       w);
        }

        // number of octaves of fbm
        #define NUM_NOISE_OCTAVES 5

        //////////////////////////////////////////////////////////////////////////////////////
        // Noise functions:
        //////////////////////////////////////////////////////////////////////////////////////

        // Precision-adjusted variations of https://www.shadertoy.com/view/4djSRW
        float hash(float p) { p = frac(p * 0.011); p *= p + 7.5; p *= p + p; return frac(p); }

        float noise(float3 x) {
            const float3 step = float3(110, 241, 171);
            float3 i = floor(x);
            float3 f = frac(x);
            float n = dot(i, step);
            float3 u = f * f * (3.0 - 2.0 * f);
            return lerp(lerp(lerp( hash(n + dot(step, float3(0, 0, 0))), hash(n + dot(step, float3(1, 0, 0))), u.x),
                           lerp( hash(n + dot(step, float3(0, 1, 0))), hash(n + dot(step, float3(1, 1, 0))), u.x), u.y),
                       lerp(lerp( hash(n + dot(step, float3(0, 0, 1))), hash(n + dot(step, float3(1, 0, 1))), u.x),
                           lerp( hash(n + dot(step, float3(0, 1, 1))), hash(n + dot(step, float3(1, 1, 1))), u.x), u.y), u.z);
        }

        float fbm(float3 x) {
	        float v = 0.0;
	        float a = 0.5;
            const float3 shift = float3(100, 100, 100);
	        for (int i = 0; i < NUM_NOISE_OCTAVES; ++i) {
		        //v += a * noise(x);
                v += a * (Noise3D(x) * .5 + .5);
		        x = x * 2.0 + shift;
		        a *= 0.5;
	        }
	        return v;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Visualization:
        //////////////////////////////////////////////////////////////////////////////////////

        static const float pi          = 3.1415926535;
        static const float inf         = 9999999.9;
        float square(float x) { return x * x; }
        float infIfNegative(float x) { return (x >= 0.0) ? x : inf; }

        // C = sphere center, r = sphere radius, P = ray origin, w = ray direction
        float intersectSphere(float3 C, float r, float3 P, float3 w) {	
	        float3 v = P - C;
	        float b = -dot(w, v);
	        float c = dot(v, v) - square(r);
	        float d = (square(b) - c);
	        if (d < 0.0) { return inf; }	
	        float dsqrt = sqrt(d);
	        
	        // Choose the first positive intersection
	        return min(infIfNegative((b - dsqrt)), infIfNegative((b + dsqrt)));
        }

        // returns max of a single float3
        float max3 (float3 v) {
          return max (max (v.x, v.y), v.z);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
            float4 color = 0.0;
        
            float3 x = IN.objPos;
            // calculate fbm noise (3 steps)
            const float3 q = float3(fbm(x + 0.025 * _Time.y + randomSeed * 0.1), fbm(x), fbm(x));
            const float3 r = float3(fbm(x + 1.0 * q + 0.01 * _Time.y), fbm(x + q), fbm(x + q));
            const float v = fbm(x + 5.0 * r + _Time.y * 0.005);
        
            
            // lerp mid color based on intermediate results
            float3 col_mid = lerp(col_mid1, col_mid2, clamp(r, 0.0, 1.0));
            col_mid = lerp(col_mid, col_mid3, clamp(q, 0.0, 1.0));
            col_mid = col_mid;
        
            // calculate pos (scaling betwen top and bot color) from v
            float pos = v * 2.0 - 1.0;
            color.xyz = lerp(col_mid, col_top, clamp(pos, 0.0, 1.0));
            color.xyz = lerp(color, col_bot, clamp(-pos, 0.0, 1.0));
        
            // clamp color to scale the highest r/g/b to 1.0
            color = color / max3(color);
              
            // create output color, increase light > 0.5 (and add a bit to dark areas)
            //color = clamp((clamp((0.4 * pow(v,3.) + pow(v,2.) + 0.5*v), 0.0, 1.0) * 0.9) * color, 0.0, 1.0);
            color = clamp((clamp((0.4 * pow(v,3.) + pow(v,2.) + 0.2*v), 0.0, 1.0) * 0.9) * color, 0.0, 1.0);
            
            o.Albedo = color;
        }
        
        ENDCG
    }
    FallBack "Diffuse"
}
