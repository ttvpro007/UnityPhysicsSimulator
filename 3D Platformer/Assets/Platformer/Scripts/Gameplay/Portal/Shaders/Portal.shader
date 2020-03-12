// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Portal"
{
	Properties
	{
		_Active("Active", Int) = 1
	}
	SubShader
	{
		//Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Tags { "RenderType" = "Opaque" }
		Cull Off
		ZWrite On
		ZTest Less
		
		Fog{ Mode Off }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}
			
			sampler2D _MainTex;
			int _Active;

			fixed4 frag (v2f i) : SV_TARGET
			{
				float2 screenSpaceUV = i.screenPos.xy / i.screenPos.w;
				return tex2D(_MainTex, screenSpaceUV) * _Active;
			}
			ENDCG
		}
	}
}
