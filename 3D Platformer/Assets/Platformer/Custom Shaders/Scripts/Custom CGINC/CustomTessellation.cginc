#ifndef CUSTOM_TESSELLATION_INCLUDED
#define CUSTOM_TESSELLATION_INCLUDED

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

TessellationControlPoint CustomTessellationVertexProgram(vertexData v)
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

TessellationFactors CustomPatchConstantFunction(InputPatch<TessellationControlPoint, 3> patch)
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
[UNITY_patchconstantfunc("CustomPatchConstantFunction")]

TessellationControlPoint CustomHullProgram(InputPatch<TessellationControlPoint, 3> patch,
	uint id : SV_OutputControlPointID)
{
	return patch[id];
}

#define CUSTOM_DOMAIN_INTERPOLATE(fieldname) data.fieldname =\
                patch[0].fieldname * barycentricCoordinates.x +\
                patch[1].fieldname * barycentricCoordinates.y +\
                patch[2].fieldname * barycentricCoordinates.z;

[UNITY_domain("tri")]
vertexOutput MyDomainProgram(TessellationFactors factors,
	OutputPatch<TessellationControlPoint, 3> patch,
	float3 barycentricCoordinates : SV_DomainLocation)
{
	vertexData data;
	CUSTOM_DOMAIN_INTERPOLATE(vertex)
		CUSTOM_DOMAIN_INTERPOLATE(normal)
		CUSTOM_DOMAIN_INTERPOLATE(tangent)
		CUSTOM_DOMAIN_INTERPOLATE(uv)
		CUSTOM_DOMAIN_INTERPOLATE(uv1)
		CUSTOM_DOMAIN_INTERPOLATE(uv2)
		return vert(data);
}

#endif // CUSTOM_TESSELLATION_INCLUDED