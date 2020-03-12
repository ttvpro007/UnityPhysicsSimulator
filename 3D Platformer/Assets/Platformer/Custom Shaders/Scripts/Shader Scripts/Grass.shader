// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Grass"
{
    Properties
    {
		_WindTex ("Wind Texture", 2D) = "white" {}
		_WindStrength ("Wind Strength", Range(0, 1)) = 0.1
		_WindSpeed ("Wind Speed", Float) = 0.05
		_BottomColor ("Bottom Color", Color) = (1, 1, 1, 1)
		_TopColor ("Top Color", Color) = (1, 1, 1, 1)
		_BendAngle ("Bend Angle", Range(0, 1)) = 0.2
		_BladeHeight ("Blade Height", Range(0, 1)) = 0.2
		_BladeHeightRandom ("Blade Height Randomness", Range(0, 1)) = 0.2
		_BladeWidth ("Blade Width", Range(0, 1)) = 0.2
		_BladeWidthRandom ("Blade Width Randomness", Range(0, 1)) = 0.2
		_TessellationUniformEdge ("Tessellation Uniform Edge", Range(0, 64)) = 0.2
		_TessellationUniformInside ("Tessellation Uniform Inside", Range(0, 64)) = 0.2
	}
    SubShader
    {
		Tags {"RenderType"="Opaque"}
		Cull Off
        Pass
        {
            CGPROGRAM
			#pragma target 4.6
            #pragma fragment frag
			#pragma geometry geo
			#pragma vertex MyTessellationVertexProgram
			#pragma hull MyHullProgram
			#pragma domain MyDomainProgram

            #include "UnityCG.cginc"

			int _TessellationUniformEdge;
			int _TessellationUniformInside;
			
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

			vertexOutput vert(vertexData v)
			{
				vertexOutput o;
				o.vertex = v.vertex;
				o.normal = v.normal;
				o.tangent = v.tangent;
				o.uv = v.uv;
				return o;
			}

			#include "MyCustomTessellation.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			float rand(float3 co)
			{
				return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
			}
			
			float3x3 AngleAxis3x3(float angle, float3 axis)
			{
				float sine = sin(angle * 0.5);
				float cosine = cos(angle * 0.5);

				// quaternion = (qx, qy, qz, qw);
				float qx = axis.x * sine;
				float qy = axis.y * sine;
				float qz = axis.z * sine;
				float qw = cosine;

				return float3x3 (
					1 - 2 * qy * qy - 2 * qz * qz, 2 * qx * qy - 2 * qz * qw, 2 * qx * qz + 2 * qy * qw,
					2 * qx * qy + 2 * qz * qw, 1 - 2 * qx * qx - 2 * qz * qz, 2 * qy * qz - 2 * qx * qw,
					2 * qx * qz - 2 * qy * qw, 2 * qy * qz + 2 * qx * qw, 1 - 2 * qx * qx - 2 * qy * qy
					);
			}

			struct geometryOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD1;
				float4 tangent : TEXCOORD2;
			};

			sampler2D _WindTex;
			float4 _WindTex_ST;
			float _WindStrength;
			float _WindSpeed;
			float4 _BottomColor;
			float4 _TopColor;
			float _BendAngle;
			float _BladeHeight;
			float _BladeHeightRandom;
			float _BladeWidth;
			float _BladeWidthRandom;

			void CalculateGeometryOutput(vertexOutput IN, inout TriangleStream<geometryOutput> triStream)
			{
				geometryOutput o;

				float2 uv = TRANSFORM_TEX(IN.uv, _WindTex);
				uv += _WindSpeed * float2(_Time.x, _Time.y);
				float2 windVelocity = _WindStrength * tex2Dlod(_WindTex, float4(uv, 0, 0));

				float3 vNormal = IN.normal;
				float4 vTangent = IN.tangent;
				float3 vBinormal = cross(vNormal, vTangent.xyz) * vTangent.w;

				float3x3 tangentToLocal = float3x3(
					vTangent.x, vBinormal.x, vNormal.x,
					vTangent.y, vBinormal.y, vNormal.y,
					vTangent.z, vBinormal.z, vNormal.z
					);

				float angleX = -((windVelocity.y - 0.5) * 2 * _WindStrength + 0.5) * UNITY_PI + UNITY_PI * 0.5;
				//float angleX = (0.5 - (windVelocity.y - 0.5) * 2 * _WindStrength + 0.5) * UNITY_PI;
				float3x3 rotationMatrixX = AngleAxis3x3(angleX, float3(1, 0, 0));

				float angleZ = rand(IN.vertex.xyz) * UNITY_TWO_PI;
				angleZ += windVelocity.x * UNITY_TWO_PI;
				float3x3 rotationMatrixZ = AngleAxis3x3(angleZ, float3(0, 0, 1));

				float3x3 transformationMatrix = mul(tangentToLocal, mul(rotationMatrixX, rotationMatrixZ));

				float width = _BladeWidth + rand(IN.vertex.zyx) * _BladeWidthRandom;
				float height = _BladeHeight + rand(IN.vertex.zyx) * _BladeHeightRandom;

				//o.pos = UnityObjectToClipPos(IN[0]);
				o.pos = UnityObjectToClipPos(IN.vertex);
				o.uv = float2(0, 0);
				triStream.Append(o);

				//o.pos = UnityObjectToClipPos(IN[1]);
				o.pos = UnityObjectToClipPos(IN.vertex + float4(mul(transformationMatrix, float3(width * 0.5, 0, height)), 1));
				o.uv = float2(0.5, 1);
				triStream.Append(o);

				//o.pos = UnityObjectToClipPos(IN[2]);
				o.pos = UnityObjectToClipPos(IN.vertex + float4(mul(transformationMatrix, float3(width, 0, 0)), 1));
				o.uv = float2(1, 0);
				triStream.Append(o);

				/*o.pos = UnityObjectToClipPos((IN[0] + IN[1] + IN[2]) / 3.0 + float4(0, 1, 0, 1));
				triStream.Append(o);*/
			}

			//v2f vert(appdata v)
			//{
			//	v2f o;
			//	o.vertex = v.vertex;
			//	o.normal = v.normal;
			//	o.tangent = v.tangent;
			//	o.uv = TRANSFORM_TEX(v.uv, _WindTex);
			//	return o;
			//}

			[maxvertexcount(9)]
			void geo(triangle vertexOutput IN[3] : SV_POSITION, inout TriangleStream<geometryOutput> triStream)
			{
				CalculateGeometryOutput(IN[0], triStream);
				CalculateGeometryOutput(IN[1], triStream);
				CalculateGeometryOutput(IN[2], triStream);
			}

			fixed4 frag(geometryOutput i) : SV_Target
			{
				//// sample the texture
				//fixed4 col = tex2D(_WindTex, i.uv);
				//return col;
				return lerp(_BottomColor, _TopColor, i.uv.y);
			}
			ENDCG
         }
    }
}
