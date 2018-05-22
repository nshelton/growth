Shader "PavelKouril/Marching Cubes/Procedural Geometry"
{	
	Properties
    {
        _WireframeColor("Wireframe", Range(0, 1)) = 0
        _NormalColor("NormalColor", Range(0, 1)) = 0
        _PhongShade("Phong", Range(0, 1)) = 0
        _AO("ao", Range(0, 1)) = 0
    }

	SubShader
	{
		Cull Off
		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float _WireframeColor;
			float _NormalColor;
			float _PhongShade;
			float _AO;

			struct Vertex
			{
				float3 vPosition;
				float3 vNormal;
			};

			struct Triangle
			{
				Vertex v[3];
			};

			uniform StructuredBuffer<Triangle> triangles;
			uniform float4x4 model;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 triID : TANGENT;
			};

			v2f vert(uint id : SV_VertexID)
			{
				uint pid = id / 3;
				uint vid = id % 3;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_VP, mul(model, float4(triangles[pid].v[vid].vPosition, 1)));
				o.normal = mul(unity_ObjectToWorld, triangles[pid].v[vid].vNormal);
				o.triID = float3(vid == 0 ? 1 : 0, vid == 1 ? 1 : 0, vid == 2 ? 1 : 0);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float d = abs(dot(normalize(WorldSpaceViewDir(i.vertex)), normalize(i.normal)));

				float wireframe = 0;

				if (i.triID.r < 0.1 || i.triID.g < 0.1 || i.triID.b < 0.1)
					wireframe = 1.0;
				
				if ( _WireframeColor > 0.5)
					clip(wireframe - 0.5);

				float3 phongShade = float3(d, d, d) ;

			 	float3 wireframeColor =  (float3) wireframe ;
				


				float3 result = 	(1.0 - _AO) *
									(phongShade + (1.0 - phongShade) * (1.0 - _PhongShade)) * 
									(abs(i.normal) + (1.0 - abs(i.normal)) * (1.0 - _NormalColor));

				return float4( result, 1.0);
			}
			ENDCG
		}
	}
}
