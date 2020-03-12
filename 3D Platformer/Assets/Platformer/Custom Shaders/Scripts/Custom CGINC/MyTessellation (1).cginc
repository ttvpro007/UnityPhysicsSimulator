#ifndef MY_CUSTOM_TESSELATION
#define MY_CUSTOM_TESSELATION

#pragma vertex MyTessellationVertexProgram
#pragma hull MyHullProgram
#pragma domain MyDomainProgram

struct TessellationControlPoint {
	float4 vertex : INTERNALTESSPOS;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
};

struct TessellationFactors {
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

struct vertexData
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
};

struct vertexOutput
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
	float3 normal : TEXCOORD1;
	float4 tangent : TEXCOORD2;
};

TessellationControlPoint MyTessellationVertexProgram(vertexData v)
{
	TessellationControlPoint o;
	o.vertex = v.vertex;
	o.normal = v.normal;
	o.tangent = v.tangent;
	o.uv = v.uv;
	o.uv1 = v.uv1;
	o.uv2 = v.uv2;
	return o;
}

vertexOutput vert(vertexData v)
{
	vertexOutput o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.normal = v.normal;
	o.tangent = v.tangent;
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	return o;
}

TessellationFactors MyPatchConstantFunction(InputPatch<TessellationControlPoint, 3> patch)
{
	TessellationFactors f;
	f.edge[0] = _TessellationUniformEdge;
	f.edge[1] = _TessellationUniformEdge;
	f.edge[2] = _TessellationUniformEdge;
	f.inside = _TessellationUniformInside;
	return f;
}

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("integer")]
[UNITY_patchconstantfunc("MyPatchConstantFunction")]

TessellationControlPoint MyHullProgram(InputPatch<TessellationControlPoint, 3> patch,
	uint id : SV_OutputControlPointID)
{
	return patch[id];
}

#define MY_DOMAIN_INTERPOLATE(fieldname) data.fieldname =\
                patch[0].fieldname * barycentricCoordinates.x +\
                patch[1].fieldname * barycentricCoordinates.y +\
                patch[2].fieldname * barycentricCoordinates.z;

[UNITY_domain("tri")]
vertexOutput MyDomainProgram(TessellationFactors factors,
	OutputPatch<TessellationControlPoint, 3> patch,
	float3 barycentricCoordinates : SV_DomainLocation)
{
	vertexData data;
	MY_DOMAIN_INTERPOLATE(vertex)
		MY_DOMAIN_INTERPOLATE(normal)
		MY_DOMAIN_INTERPOLATE(tangent)
		MY_DOMAIN_INTERPOLATE(uv)
		MY_DOMAIN_INTERPOLATE(uv1)
		MY_DOMAIN_INTERPOLATE(uv2)
		return vert(data);
}

#endif // UNITY_CG_INCLUDED
