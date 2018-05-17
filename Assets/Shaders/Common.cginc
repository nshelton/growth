#define UNITY_PI 3.1415926


// PRNG function
float nrand(float2 uv, float salt)
{
    uv += salt;
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}


// Deterministic random rotation axis
float3 random_on_sphere(float2 uv)
{
    // Uniformaly distributed points
    // http://mathworld.wolfram.com/SpherePointPicking.html
    float u = nrand(uv, 10) * 2 - 1;
    float theta = nrand(uv, 11) * UNITY_PI * 2;
    float u2 = sqrt(1 - u * u);
    return float3(u2 * cos(theta), u2 * sin(theta), u);
}
