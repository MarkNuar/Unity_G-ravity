static const float PI = 3.14159265359;
static const float TAU = PI * 2;
static const float maxFloat = 3.402823466e+38;

// Remap a value from one range to another
float remap(float v, float minOld, float maxOld, float minNew, float maxNew) {
    return saturate(minNew + (v-minOld) * (maxNew - minNew) / (maxOld-minOld));
}

// Remap the components of a vector from one range to another
float4 remap(float4 v, float minOld, float maxOld, float minNew, float maxNew) {
    return saturate(minNew + (v-minOld) * (maxNew - minNew) / (maxOld-minOld));//
}

// Remap a float value (with a known mininum and maximum) to a value between 0 and 1
float remap01(float v, float minOld, float maxOld) {
    return saturate((v-minOld) / (maxOld-minOld));
}

// Remap a float2 value (with a known mininum and maximum) to a value between 0 and 1
float2 remap01(float2 v, float2 minOld, float2 maxOld) {
    return saturate((v-minOld) / (maxOld-minOld));
}

// Smooth minimum of two values, controlled by smoothing factor k
// When k = 0, this behaves identically to min(a, b)
float smoothMin(float a, float b, float k) {
    k = max(0, k);
    // https://www.iquilezles.org/www/articles/smin/smin.htm
    float h = max(0, min(1, (b - a + k) / (2 * k)));
    return a * h + b * (1 - h) - k * h * (1 - h);
}

// Smooth maximum of two values, controlled by smoothing factor k
// When k = 0, this behaves identically to max(a, b)
float smoothMax(float a, float b, float k) {
    k = min(0, -k);
    float h = max(0, min(1, (b - a + k) / (2 * k)));
    return a * h + b * (1 - h) - k * h * (1 - h);
}

float Blend(float startHeight, float blendDst, float height) {
    return smoothstep(startHeight - blendDst / 2, startHeight + blendDst / 2, height);
}



// Returns dstToSphere, dstThroughSphere
// If inside sphere, dstToSphere will be 0
// If ray misses sphere, dstToSphere = max float value, dstThroughSphere = 0
// Given rayDir must be normalized
float2 ray_sphere(float3 centre, float radius, float3 rayOrigin, float3 rayDir) {
    float3 offset = rayOrigin - centre;
    const float a = 1; // set to dot(rayDir, rayDir) instead if rayDir may not be normalized
    float b = 2 * dot(offset, rayDir);
    float c = dot (offset, offset) - radius * radius;

    float discriminant = b*b-4*a*c;
    // No intersections: discriminant < 0
    // 1 intersection: discriminant == 0
    // 2 intersections: discriminant > 0
    if (discriminant > 0) {
        float s = sqrt(discriminant);
        float dstToSphereNear = max(0, (-b - s) / (2 * a));
        float dstToSphereFar = (-b + s) / (2 * a);

        if (dstToSphereFar >= 0) {
            return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
        }
    }
	 // Ray did not intersect sphere
    return float2(maxFloat, 0);
}

// source
// https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
// n = plane normal -> inclination of planet
// p0 = point on plane -> center of planet
// l0 = ray origin -> view ray origin
// l = ray direction -> view direction
// t = distance from l0 to intersection with plane
// return if ray intersects plane 
float intersect_plane(float3 n, float3 p0, float3 l0, float3 l, out float t)
{
    const float denominator = dot(n, l);
    if(abs(denominator) > 1e-6)
    {
        const float3 p0_l0 = p0 - l0;
        t = dot(p0_l0, n) / denominator;
        return (t >= 0);
    }
    return false;
}

// n = plane normal -> inclination of planet
// p0 = point on plane -> center of planet
// r = radius of the disk
// l0 = ray origin -> view ray origin
// l = ray direction -> view direction
// t = distance from l0 to intersection with plane
// return if ray intersects disk 
bool intersect_disk(float3 n, float3 p0, float r, float3 l0, float3 l, out float t, out float d)
{
    if(intersect_plane(n, p0, l0, l, t))
    {
        const float3 p = l0 + l * t;
        const float3 v = p - p0;
        d = length(v);
        const float d2 = dot(v, v);
        const float r2 = r * r;
        return (d2 <= r2);
    }
    return false;
}

// n = plane normal -> inclination of planet
// p0 = point on plane -> center of planet
// r_inner = radius of inner disk
// r_outer = radius of outer disk
// l0 = ray origin -> view ray origin
// l = ray direction -> view direction
// return distance at which intersection occurs (from point of view) and distance from center of ring
float2 intersect_ring(float3 n, float3 p0, float r_inner, float r_outer, float3 l0, float3 l)
{
    float t_outer = 0;
    float t_inner = 0;
    float d_outer = 0;
    float d_inner = 0;
    const bool intersect_outer_disk = intersect_disk(n, p0, r_outer, l0, l, t_outer, d_outer);
    const bool intersect_inner_disk = intersect_disk(n, p0, r_inner, l0, l, t_inner, d_inner);
    if(intersect_outer_disk && !intersect_inner_disk)
    {
        return float2(t_outer, d_outer);
    }
    return float2(maxFloat, maxFloat);
}