Shader "Custom/SurfaceBuffer" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows addshadow
        #pragma multi_compile_instancing
		#pragma target 5.0

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		sampler2D _MainTex;
		int numTris;

		struct Vertex
		{
			float3 vPosition;
			float3 vNormal;
		};

		struct Triangle
		{
			Vertex v[3];
		};


#ifdef SHADER_API_D3D11
		StructuredBuffer<Triangle> _TriangleBuffer;
#endif

		struct Input {
			float2 uv_MainTex;
            float4 color : COLOR;
		};

		struct appdata
		{
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 color : COLOR;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            float4 texcoord2 : TEXCOORD2;
 
            uint id : SV_VertexID;
            uint instanceID : SV_InstanceID;
		};
	
		void vert (inout appdata v )
		{

#ifdef SHADER_API_D3D11
			int vertexid =  v.id;

			v.vertex.xyz = _TriangleBuffer[vertexid / 3].v[vertexid % 3].vPosition;
			v.normal.xyz = _TriangleBuffer[vertexid / 3].v[vertexid % 3].vNormal;

			v.color =  float4(v.instanceID % 2,  v.instanceID %3,  v.instanceID %5, 1.0);
#endif
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {

			// Albedo comes from a texture tinted by color
			o.Albedo = _Color.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
