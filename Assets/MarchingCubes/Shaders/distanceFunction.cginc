
float4 _radius;
float _Time;

float DE(float3 p)
{
    float2 t = _radius.xy;
    float2 q = float2(length(p.xz)-t.x,p.y);
    float amount =  length(q)-t.y;

    amount += 0.05 * sin(dot(p, (float3)1.0) * _radius.z + _Time);
     amount += 0.04 * sin(p.z * 10.0 * sin(p.x*_radius.w) + _Time) *  sin(p.x * _radius.w + _Time) *  sin(p.y * _radius.w + _Time);
    return amount;
}